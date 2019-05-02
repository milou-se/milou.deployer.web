using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    [UsedImplicitly]
    public class HookService
    {
        private readonly DeploymentService _deploymentService;

        private readonly ILogger _logger;

        private readonly MonitoringService _monitoringService;

        private readonly IDeploymentTargetReadService _targetSource;

        public HookService(
            IDeploymentTargetReadService targetSource,
            DeploymentService deploymentService,
            ILogger logger,
            MonitoringService monitoringService)
        {
            _targetSource = targetSource;
            _deploymentService = deploymentService;
            _logger = logger;
            _monitoringService = monitoringService;
        }

        public async Task AutoDeployAsync([NotNull] IReadOnlyCollection<PackageVersion> packageIdentifiers)
        {
            if (packageIdentifiers == null)
            {
                throw new ArgumentNullException(nameof(packageIdentifiers));
            }

            _logger.Information("Received hook for packages {Packages}",
                string.Join(", ", packageIdentifiers.Select(p => p.ToString())));

            var deploymentTargets =
                (await _targetSource.GetDeploymentTargetsAsync(CancellationToken.None))
                .SafeToReadOnlyCollection();

            var withAutoDeploy = deploymentTargets.Where(t => t.AutoDeployEnabled).ToArray();

            if (!withAutoDeploy.Any())
            {
                _logger.Information("No target has auto deploy enabled");
            }
            else
            {
                foreach (var deploymentTarget in withAutoDeploy)
                {
                    foreach (var packageIdentifier in packageIdentifiers)
                    {
                        if (deploymentTarget.PackageId.Equals(
                            packageIdentifier.PackageId,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            var allowDeployment =
                                !packageIdentifier.Version.IsPrerelease || deploymentTarget.AllowPreRelease;

                            if (allowDeployment)
                            {
                                var metadata = await _monitoringService.GetAppMetadataAsync(
                                    deploymentTarget,
                                    CancellationToken.None);

                                if (metadata.SemanticVersion != null)
                                {
                                    if (packageIdentifier.Version > metadata.SemanticVersion)
                                    {
                                        _logger.Information(
                                            "Auto deploying package {PackageIdentifier} to target {Name} from web hook",
                                            packageIdentifier,
                                            deploymentTarget.Name);

                                        var result =
                                            await
                                                _deploymentService.ExecuteDeploymentAsync(
                                                    new DeploymentTask(
                                                        $"{packageIdentifier.PackageId}, {packageIdentifier.Version.ToNormalizedString()}",
                                                        deploymentTarget.Id,
                                                        Guid.NewGuid(),
                                                        "Web hook"
                                                    ),
                                                    _logger,
                                                    CancellationToken.None);

                                        _logger.Information(
                                            "Deployed package {PackageIdentifier} to target {Name} from web hook with result {Result}",
                                            packageIdentifier,
                                            deploymentTarget.Name,
                                            result);
                                    }
                                    else
                                    {
                                        _logger.Information(
                                            "Auto deployment skipped for {PackageIdentifier} since deployed version is higher {V}",
                                            packageIdentifier,
                                            metadata.SemanticVersion.ToNormalizedString());
                                    }
                                }
                                else
                                {
                                    _logger.Information(
                                        "Auto deployment skipped for {PackageIdentifier} since the target version could not be determined",
                                        packageIdentifier);
                                }
                            }
                            else
                            {
                                _logger.Information(
                                    "Auto deployment skipped for {PackageIdentifier} since the target does not allow pre-release",
                                    packageIdentifier);
                            }
                        }
                        else
                        {
                            _logger.Information("No package id matched {PackageIdentifier} for target {Name}",
                                packageIdentifier,
                                deploymentTarget.Name);
                        }
                    }
                }
            }
        }
    }
}
