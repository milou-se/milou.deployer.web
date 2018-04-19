using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    [Route(BaseRoute)]
    public class MonitoringController : BaseApiController
    {
        public const string BaseRoute = "monitoring";
        private readonly DeploymentService _deploymentService;

        private readonly MonitoringService _monitoringService;

        private readonly IDeploymentTargetReadService _targetSource;

        public MonitoringController(
            MonitoringService monitoringService,
            IDeploymentTargetReadService targetSource,
            DeploymentService deploymentService)
        {
            _monitoringService = monitoringService;
            _targetSource = targetSource;
            _deploymentService = deploymentService;
        }

        [Route("~/")]
        [Route("")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<DeploymentTarget> readOnlyCollection =
                (await _targetSource.GetOrganizationsAsync(cancellationToken)).SelectMany(
                    organization => organization.Projects.SelectMany(project => project.DeploymentTargets))
                .SafeToReadOnlyCollection();

            Task<IReadOnlyCollection<AppVersion>> task =
                _monitoringService.GetAppMetadataAsync(readOnlyCollection, cancellationToken);

            Task packages = _deploymentService.GetPackageVersionsAsync();

            await Task.WhenAll(packages, task);

            IReadOnlyCollection<AppVersion> appVersions = await task;

            return View(new MonitoringViewOutputModel(appVersions));
        }
    }
}