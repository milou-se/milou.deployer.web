using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
                .Select(t => new
                {
                    targetId = t.Id,
                    name = t.Name,
                    url = t.Url,
                    editUrl = Url.RouteUrl(TargetConstants.EditTargetRouteName, new { deploymentTargetId = t.Id }),
                    historyUrl = Url.RouteUrl(DeploymentConstants.HistoryRouteName, new { deploymentTargetId = t.Id })
                });

            return Json(new { targets = targets });
        }
    }
}
