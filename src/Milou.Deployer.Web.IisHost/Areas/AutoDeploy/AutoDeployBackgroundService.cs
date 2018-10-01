using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.AutoDeploy
{
    [UsedImplicitly]
    public class AutoDeployBackgroundService : BackgroundService
    {
        private readonly AutoDeployConfiguration _autoDeployConfiguration;
        private readonly DeploymentService _deploymentService;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly DeploymentWorker _deploymentWorker;
        private readonly MonitoringService _monitoringService;
        private ILogger _logger;
        private PackageService _packageService;

        public AutoDeployBackgroundService(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] DeploymentService deploymentService,
            [NotNull] MonitoringService monitoringService,
            [NotNull] DeploymentWorker deploymentWorker,
            [NotNull] AutoDeployConfiguration autoDeployConfiguration,
            [NotNull] ILogger logger,
            [NotNull] PackageService packageService)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _deploymentWorker = deploymentWorker ?? throw new ArgumentNullException(nameof(deploymentWorker));
            _autoDeployConfiguration = autoDeployConfiguration ??
                                       throw new ArgumentNullException(nameof(autoDeployConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_autoDeployConfiguration.Enabled)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(_autoDeployConfiguration.StartupDelayInSeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                ImmutableArray<DeploymentTarget> deploymentTargets;
                using (var targetsTokenSource =
                    new CancellationTokenSource(TimeSpan.FromSeconds(_autoDeployConfiguration.DefaultTimeoutInSeconds)))
                {
                    using (CancellationTokenSource linked =
                        CancellationTokenSource.CreateLinkedTokenSource(stoppingToken,
                            targetsTokenSource.Token))
                    {
                        deploymentTargets = (await _deploymentTargetReadService.GetDeploymentTargetsAsync(linked.Token))
                            .Where(target => target.AutoDeployEnabled)
                            .ToImmutableArray();
                    }
                }

                if (deploymentTargets.IsDefaultOrEmpty)
                {
                    continue;
                }

                ImmutableArray<DeploymentTarget> targetsWithUrl = deploymentTargets.Where(target => target.Url.HasValue()).ToImmutableArray();

                AppVersion[] appVersions;
                using (var cancellationTokenSource =
                    new CancellationTokenSource(
                        TimeSpan.FromSeconds(_autoDeployConfiguration.MetadataTimeoutInSeconds)))
                {
                    using (CancellationTokenSource linkedCancellationTokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, stoppingToken))
                    {
                        CancellationToken cancellationToken = linkedCancellationTokenSource.Token;

                        IEnumerable<Task<AppVersion>> tasks = targetsWithUrl.Select(target =>
                            _monitoringService.GetAppMetadataAsync(target, cancellationToken));

                        appVersions = await Task.WhenAll(tasks);
                    }
                }

                foreach (DeploymentTarget deploymentTarget in targetsWithUrl)
                {
                    AppVersion appVersion = appVersions.SingleOrDefault(v =>
                        v.Target.Id.Equals(deploymentTarget.Id, StringComparison.OrdinalIgnoreCase));

                    if (appVersion is null ||
                        appVersion.SemanticVersion is null ||
                        appVersion.PackageId.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    ImmutableHashSet<PackageVersion> packageVersions;
                    using (var packageVersionCancellationTokenSource =
                        new CancellationTokenSource(TimeSpan.FromSeconds(_autoDeployConfiguration.DefaultTimeoutInSeconds)))
                    {
                        using (CancellationTokenSource linked =
                            CancellationTokenSource.CreateLinkedTokenSource(stoppingToken,
                                packageVersionCancellationTokenSource.Token))
                        {
                            packageVersions =
                                (await _packageService.GetPackageVersionsAsync(deploymentTarget.PackageId, cancellationToken: linked.Token, logger: _logger)).ToImmutableHashSet();
                        }
                    }

                    if (packageVersions.IsEmpty)
                    {
                        continue;
                    }

                    ImmutableHashSet<PackageVersion> filteredPackages;
                    if (!deploymentTarget.AllowPrerelease)
                    {
                        filteredPackages = packageVersions.Where(p => !p.Version.IsPrerelease).ToImmutableHashSet();
                    }
                    else
                    {
                        filteredPackages = packageVersions;
                    }

                    if (filteredPackages.IsEmpty)
                    {
                        continue;
                    }

                    ImmutableHashSet<PackageVersion> newerPackages = filteredPackages
                        .Where(package =>
                            package.PackageId.Equals(appVersion.PackageId, StringComparison.OrdinalIgnoreCase) &&
                            package.Version > appVersion.SemanticVersion)
                        .ToImmutableHashSet();

                    PackageVersion packageToDeploy = newerPackages
                        .OrderByDescending(package => package.Version)
                        .FirstOrDefault();

                    if (packageToDeploy != null)
                    {
                        var task = new DeploymentTask(packageToDeploy, deploymentTarget.Id, Guid.NewGuid());

                        _logger.Information("Auto-deploying package {Package} to target {TargetId}",
                            packageToDeploy,
                            deploymentTarget.Id);

                        _deploymentWorker.Enqueue(task);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_autoDeployConfiguration.AfterDeployDelayInSeconds), stoppingToken);
            }
        }
    }
}