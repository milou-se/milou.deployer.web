using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
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
        [Route("~/")]
        [Route("")]
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
        [Route("~/status")]
        public async Task<IActionResult> Status(CancellationToken cancellationToken)
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
                        latestNewerAvailabe = ""
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

            return Json(new
            {
                displayName = appVersion.Status.DisplayName,
                key = appVersion.Status.Key,
                semanticVersion = appVersion.SemanticVersion?.ToNormalizedString().WithDefault(Constants.NotAvailable),
                isPreReleaseVersion = appVersion.SemanticVersion?.IsPrerelease ?? false,
                preReleaseClass = appVersion.PreReleaseClass,
                intervalAgo = appVersion.DeployedAtUtc.RelativeUtcToLocalTime(clock),
                intervalAgoName = deploymentInterval.Name,
                deployedAtLocalTime = appVersion.DeployedAtUtc.ToLocalTimeFormatted(clock),
                statusMessage = appVersion.Message,
                latestNewerAvailable = appVersion.LatestNewerAvailable?.ToNormalizedString() ?? ""
            });
        }
    }
}
