using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Time;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Areas.Targets.Controllers;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    [Route(BaseRoute)]
    public class MonitoringController : BaseApiController
    {
        public const string BaseRoute = "monitoring";

        private readonly MonitoringService _monitoringService;

        private readonly IDeploymentTargetReadService _targetSource;

        public MonitoringController(
            MonitoringService monitoringService,
            IDeploymentTargetReadService targetSource)
        {
            _monitoringService = monitoringService;
            _targetSource = targetSource;
        }

        [HttpGet]
        [Route("~/status")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var targets = (await _targetSource.GetOrganizationsAsync(cancellationToken))
                .SelectMany(
                    organization => organization.Projects.SelectMany(project => project.DeploymentTargets))
                .SafeToReadOnlyCollection();

            var appVersions =
                await _monitoringService.GetAppMetadataAsync(targets, cancellationToken);

            return View(new MonitoringViewOutputModel(appVersions));
        }

        [HttpGet]
        [Route(MonitorConstants.MonitorRoute, Name = MonitorConstants.MonitorRouteName)]
        [Route("")]
        public IActionResult Status()
        {
            return View();
        }

        [HttpGet]
        [Route("~/api/targets")]
        public async Task<IActionResult> Targets(CancellationToken cancellationToken)
        {
            var targets = (await _targetSource.GetOrganizationsAsync(cancellationToken))
                .SelectMany(
                    organization => organization.Projects.SelectMany(project => project.DeploymentTargets))
                .Select(deploymentTarget =>
                {
                    var editUrl = Url.RouteUrl(TargetConstants.EditTargetRouteName,
                        new { deploymentTargetId = deploymentTarget.Id });
                    var historyUrl = Url.RouteUrl(DeploymentConstants.HistoryRouteName,
                        new { deploymentTargetId = deploymentTarget.Id });
                    var statusUrl = Url.RouteUrl(TargetConstants.TargetStatusApiRouteName,
                        new { deploymentTargetId = deploymentTarget.Id });
                    return new
                    {
                        targetId = deploymentTarget.Id,
                        name = deploymentTarget.Name,
                        url = deploymentTarget.Url,
                        editUrl,
                        historyUrl,
                        statusKey = DeployStatus.Unknown.Key,
                        statusDisplayName = DeployStatus.Unknown.DisplayName,
                        statusUrl,
                        isPreReleaseVersion = false,
                        semanticVersion = "",
                        preReleaseClass = "",
                        intervalAgo = "",
                        intervalAgoName = "",
                        deployedAtLocalTime = "",
                        environmentType = deploymentTarget.EnvironmentType.Name,
                        metadataUrl = deploymentTarget.Url is null ? null : $"{deploymentTarget.Url.AbsoluteUri.TrimEnd('/')}/applicationmetadata.json",
                        statusMessage = "",
                        latestNewerAvailabe = "",
                        deployEnabled = deploymentTarget.Enabled && !deploymentTarget.IsReadOnly,
                        packages = Array.Empty<object>(),
                        packageId = deploymentTarget.PackageId
                    };
                });

            return Json(new { targets });
        }

        [HttpGet]
        [Route(TargetConstants.TargetStatusApiRoute, Name = TargetConstants.TargetStatusApiRouteName)]
        public async Task<IActionResult> Status(
            string deploymentTargetId,
            [FromServices] IDeploymentTargetReadService deploymentTargetReadService,
            [FromServices] MonitoringService monitoringService,
            [FromServices] ICustomClock clock)
        {
            var deploymentTarget = await deploymentTargetReadService.GetDeploymentTargetAsync(deploymentTargetId);

            if (deploymentTarget is null)
            {
                return new NotFoundResult();
            }

            if (deploymentTarget.Url is null)
            {
                return Json(DeployStatus.Unavailable);
            }

            if (!deploymentTarget.Enabled)
            {
                return Json(DeployStatus.Unavailable);
            }

            var appVersion = await monitoringService.GetAppMetadataAsync(deploymentTarget, default);

            var deploymentInterval = appVersion.DeployedAtUtc.IntervalAgo(clock);

            var selectedPackageIndex = appVersion.AvailablePackageVersions
                        .Select((item, index) => new
                        {
                            Selected = item.PackageId == deploymentTarget.PackageId &&
                                       item.Version == appVersion.SemanticVersion,
                            Index = index
                        }).SingleOrDefault(t => t.Selected)?.Index ?? -1;
            return Json(new
            {
                displayName = appVersion.Status.DisplayName,
                key = appVersion.Status.Key,
                semanticVersion =
                    appVersion.SemanticVersion?.ToNormalizedString().WithDefault(Constants.NotAvailable),
                isPreReleaseVersion = appVersion.SemanticVersion?.IsPrerelease ?? false,
                preReleaseClass = appVersion.PreReleaseClass,
                intervalAgo = appVersion.DeployedAtUtc.RelativeUtcToLocalTime(clock),
                intervalAgoName = deploymentInterval.Name,
                deployedAtLocalTime = appVersion.DeployedAtUtc.ToLocalTimeFormatted(clock),
                statusMessage = appVersion.Message,
                latestNewerAvailable = appVersion.LatestNewerAvailable?.ToNormalizedString() ?? "",
                deployEnabled =
                    deploymentTarget.Enabled && !deploymentTarget.IsReadOnly,
                packageId = deploymentTarget.PackageId,
                packages = appVersion.AvailablePackageVersions.Select(availableVersion => new
                {
                    packageId = availableVersion.PackageId,
                    version = availableVersion.Version.ToNormalizedString(),
                    combinedName = availableVersion.Key,
                    isNewer = availableVersion.Version > appVersion.SemanticVersion,
                    isCurrent = availableVersion.Version == appVersion.SemanticVersion,
                    preReleaseWarning = availableVersion.Version.IsPrerelease && appVersion.SemanticVersion?.IsPrerelease == false
                }).ToArray(),
                selectedPackageIndex
            });
        }
    }
}
