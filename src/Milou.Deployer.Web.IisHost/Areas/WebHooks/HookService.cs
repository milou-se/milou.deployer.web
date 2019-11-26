﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using MediatR;

using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.AutoDeploy;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    [UsedImplicitly]
    public class HookService : INotificationHandler<PackageWebHookNotification>
    {
        private readonly DeploymentWorkerService _deploymentService;

        private readonly ILogger _logger;

        private readonly MonitoringService _monitoringService;

        private readonly AutoDeployConfiguration _autoDeployConfiguration;

        private readonly IDeploymentTargetReadService _targetSource;

        public HookService(
            IDeploymentTargetReadService targetSource,
            DeploymentWorkerService deploymentService,
            ILogger logger,
            MonitoringService monitoringService,
            AutoDeployConfiguration autoDeployConfiguration)
        {
            _targetSource = targetSource;
            _deploymentService = deploymentService;
            _logger = logger;
            _monitoringService = monitoringService;
            _autoDeployConfiguration = autoDeployConfiguration;
        }

        public async Task Handle(PackageWebHookNotification notification, CancellationToken cancellationToken)
        {
            if (!_autoDeployConfiguration.Enabled)
            {
                _logger.Information("Auto deploy is disabled, skipping package web hook notification");
                return;
            }

            var packageIdentifier = notification.PackageVersion;

            if (packageIdentifier == null)
            {
                throw new ArgumentNullException(nameof(packageIdentifier));
            }

            _logger.Information("Received hook for package {Package}", packageIdentifier);

            var deploymentTargets =
                (await _targetSource.GetDeploymentTargetsAsync(stoppingToken: cancellationToken))
                .SafeToReadOnlyCollection();

            var withAutoDeploy = deploymentTargets.Where(target => target.AutoDeployEnabled).ToArray();

            if (!withAutoDeploy.Any())
            {
                _logger.Information("No target has auto deploy enabled");
            }
            else
            {
                foreach (var deploymentTarget in withAutoDeploy)
                {
                    if (deploymentTarget.PackageId.Equals(
                        packageIdentifier.PackageId,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        if (deploymentTarget.NuGet.NuGetConfigFile != null
                            && !deploymentTarget.NuGet.NuGetConfigFile.Equals(notification.NugetConfig))
                        {
                            _logger.Information("Target {Target} does not match NuGet config", deploymentTarget.Id);
                            continue;
                        }

                        if (deploymentTarget.NuGet.NuGetPackageSource != null
                            && !deploymentTarget.NuGet.NuGetPackageSource.Equals(notification.NugetSource))
                        {
                            _logger.Information("Target {Target} does not match NuGet source", deploymentTarget.Id);
                            continue;
                        }

                        bool allowDeployment =
                            !packageIdentifier.Version.IsPrerelease || deploymentTarget.AllowPreRelease;

                        if (allowDeployment)
                        {
                            var metadata = await _monitoringService.GetAppMetadataAsync(
                                               deploymentTarget,
                                               cancellationToken);

                            if (metadata.SemanticVersion != null)
                            {
                                if (packageIdentifier.Version > metadata.SemanticVersion)
                                {
                                    _logger.Information(
                                        "Auto deploying package {PackageIdentifier} to target {Name} from web hook",
                                        packageIdentifier,
                                        deploymentTarget.Name);

                                            _deploymentService.Enqueue(
                                                new DeploymentTask(
                                                    packageIdentifier,
                                                    deploymentTarget.Id,
                                                    Guid.NewGuid(),
                                                    "Web hook"));

                                    _logger.Information(
                                        "Successfully enqueued package {PackageIdentifier} to target {Name} from web hook",
                                        packageIdentifier,
                                        deploymentTarget.Name);
                                }
                                else
                                {
                                    _logger.Information(
                                        "Auto deployment skipped for {PackageIdentifier} since deployed version is higher {MetadataVersion}",
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
                        _logger.Information(
                            "No package id matched {PackageIdentifier} for target {Name}",
                            packageIdentifier,
                            deploymentTarget.Name);
                    }
                }
            }
        }
    }
}