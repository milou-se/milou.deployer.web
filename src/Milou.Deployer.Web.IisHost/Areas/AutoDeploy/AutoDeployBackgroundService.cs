using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Application.Metadata;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Settings;
using Milou.Deployer.Web.Core.Time;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.NuGet;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.AutoDeploy
{
    [UsedImplicitly]
    public class AutoDeployBackgroundService : BackgroundService
    {
        private readonly AutoDeployConfiguration _autoDeployConfiguration;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly DeploymentWorkerService _deploymentWorkerService;
        private readonly ILogger _logger;
        private readonly MonitoringService _monitoringService;
        private readonly PackageService _packageService;
        private readonly TimeoutHelper _timeoutHelper;

        private readonly IApplicationSettingsStore _applicationSettingsStore;

        public AutoDeployBackgroundService(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] MonitoringService monitoringService,
            [NotNull] DeploymentWorkerService deploymentWorkerService,
            [NotNull] AutoDeployConfiguration autoDeployConfiguration,
            [NotNull] ILogger logger,
            [NotNull] PackageService packageService,
            TimeoutHelper timeoutHelper,
            IApplicationSettingsStore applicationSettingsStore)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _deploymentWorkerService = deploymentWorkerService ??
                                       throw new ArgumentNullException(nameof(deploymentWorkerService));
            _autoDeployConfiguration = autoDeployConfiguration ??
                                       throw new ArgumentNullException(nameof(autoDeployConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
            _timeoutHelper = timeoutHelper;
            _applicationSettingsStore = applicationSettingsStore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var applicationSettings = await _applicationSettingsStore.GetApplicationSettings(stoppingToken);

            if (!applicationSettings.AutoDeploy.Enabled)
            {
                _logger.Debug("Auto deploy is disabled");
                return;
            }

            if (!applicationSettings.AutoDeploy.PollingEnabled)
            {
                _logger.Debug("Auto deploy polling is disabled");
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(_autoDeployConfiguration.StartupDelayInSeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var deploymentTargets = await GetDeploymentTargetsWithAutoDeployEnabled(stoppingToken);

                if (deploymentTargets.IsDefaultOrEmpty)
                {
                    _logger.Verbose(
                        "Found no deployment targets with auto deployment enabled, waiting {DelayInSeconds} seconds",
                        _autoDeployConfiguration.EmptyTargetsDelayInSeconds);

                    await Task.Delay(
                        TimeSpan.FromSeconds(_autoDeployConfiguration.EmptyTargetsDelayInSeconds),
                        stoppingToken);

                    continue;
                }

                var targetsWithUrl = deploymentTargets.Where(target => target.Url.HasValue()).ToImmutableArray();

                if (targetsWithUrl.IsDefaultOrEmpty)
                {
                    _logger.Verbose(
                        "Found no deployment targets with auto deployment enabled and URL defined, waiting {DelayInSeconds} seconds",
                        _autoDeployConfiguration.EmptyTargetsDelayInSeconds);

                    await Task.Delay(
                        TimeSpan.FromSeconds(_autoDeployConfiguration.EmptyTargetsDelayInSeconds),
                        stoppingToken);

                    continue;
                }

                var appVersions = await GetAppVersions(stoppingToken, targetsWithUrl);

                foreach (var deploymentTarget in targetsWithUrl)
                {
                    var appVersion = appVersions.SingleOrDefault(version =>
                        version.Target.Id.Equals(deploymentTarget.Id, StringComparison.OrdinalIgnoreCase));

                    if (appVersion?.SemanticVersion is null || appVersion.PackageId.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var packageVersions = await GetPackageVersions(stoppingToken, deploymentTarget);

                    if (packageVersions.IsEmpty)
                    {
                        continue;
                    }

                    var filteredPackages = !deploymentTarget.AllowPreRelease
                        ? packageVersions.Where(packageVersion => !packageVersion.Version.IsPrerelease).ToImmutableHashSet()
                        : packageVersions;

                    if (filteredPackages.IsEmpty)
                    {
                        _logger.Debug("Found no auto deploy versions for target {TargetId} allowing pre-release {AllowPreRelease}", deploymentTarget.Id, deploymentTarget.AllowPreRelease);
                        continue;
                    }

                    var newerPackages = filteredPackages
                        .Where(package =>
                            package.PackageId.Equals(appVersion.PackageId, StringComparison.OrdinalIgnoreCase)
                            && package.Version > appVersion.SemanticVersion)
                        .ToImmutableHashSet();

                    var packageToDeploy = newerPackages
                        .OrderByDescending(package => package.Version)
                        .FirstOrDefault();

                    if (packageToDeploy != null)
                    {
                        var task = new DeploymentTask(packageToDeploy, deploymentTarget.Id, Guid.NewGuid(), nameof(AutoDeployBackgroundService));

                        _logger.Information(
                            "Enqueuing auto deploy package {Package} to target {TargetId}",
                            packageToDeploy,
                            deploymentTarget.Id);

                        _deploymentWorkerService.Enqueue(task);
                    }
                    else
                    {
                        _logger.Debug("Found no newer auto deploy versions for target {TargetId} allowing pre-release {AllowPreRelease}", deploymentTarget.Id, deploymentTarget.AllowPreRelease);
                    }
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(_autoDeployConfiguration.AfterDeployDelayInSeconds),
                    stoppingToken);
            }
        }

        private async Task<ImmutableHashSet<PackageVersion>> GetPackageVersions(CancellationToken stoppingToken, DeploymentTarget deploymentTarget)
        {
            try
            {
                ImmutableHashSet<PackageVersion> packageVersions;
                using (var packageVersionCancellationTokenSource =
                    _timeoutHelper.CreateCancellationTokenSource(
                        TimeSpan.FromSeconds(_autoDeployConfiguration.DefaultTimeoutInSeconds)))
                {
                    using (var linked =
                        CancellationTokenSource.CreateLinkedTokenSource(
                            stoppingToken,
                            packageVersionCancellationTokenSource.Token))
                    {
                        packageVersions =
                            (await _packageService.GetPackageVersionsAsync(
                                 deploymentTarget.PackageId,
                                 cancellationToken: linked.Token,
                                 logger: _logger)).ToImmutableHashSet();
                    }
                }

                return packageVersions;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Could not get package versions for auto deploy of target {TargetId}", deploymentTarget.Id);
                return ImmutableHashSet<PackageVersion>.Empty;
            }
        }

        private async Task<AppVersion[]> GetAppVersions(CancellationToken stoppingToken, ImmutableArray<DeploymentTarget> targetsWithUrl)
        {
            try
            {
                AppVersion[] appVersions;
                using (var cancellationTokenSource =
                    _timeoutHelper.CreateCancellationTokenSource(
                        TimeSpan.FromSeconds(_autoDeployConfiguration.MetadataTimeoutInSeconds)))
                {
                    using (var linkedCancellationTokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, stoppingToken))
                    {
                        var cancellationToken = linkedCancellationTokenSource.Token;

                        var tasks = targetsWithUrl.Select(
                            target =>
                                _monitoringService.GetAppMetadataAsync(target, cancellationToken));

                        appVersions = await Task.WhenAll(tasks);
                    }
                }

                return appVersions;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Could not app versions for auto deploy");

                return Array.Empty<AppVersion>();
            }
        }

        private async Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsWithAutoDeployEnabled(CancellationToken stoppingToken)
        {
            try
            {
                ImmutableArray<DeploymentTarget> deploymentTargets;
                using (var targetsTokenSource =
                    _timeoutHelper.CreateCancellationTokenSource(
                        TimeSpan.FromSeconds(_autoDeployConfiguration.DefaultTimeoutInSeconds)))
                {
                    using (var linked =
                        CancellationTokenSource.CreateLinkedTokenSource(
                            stoppingToken,
                            targetsTokenSource.Token))
                    {
                        deploymentTargets =
                            (await _deploymentTargetReadService.GetDeploymentTargetsAsync(stoppingToken: linked.Token))
                            .Where(target => target.Enabled && target.AutoDeployEnabled)
                            .ToImmutableArray();
                    }
                }

                return deploymentTargets;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Could not get targets with auto deploy enabled");

                return ImmutableArray<DeploymentTarget>.Empty;
            }
        }
    }
}
