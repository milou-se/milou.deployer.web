using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewInputModels;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Controllers;
using NuGet.Versioning;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    public class DeployController : BaseApiController
    {
        private readonly DeploymentWorkerService _deploymentService;

        private readonly ILogger _logger;

        public DeployController(ILogger logger, DeploymentWorkerService deploymentService)
        {
            _logger = logger;
            _deploymentService = deploymentService;
        }

        [Route(DeploymentConstants.DeployRoute, Name = DeploymentConstants.DeployRouteName)]
        [HttpPost]
        public IActionResult Index(DeploymentTaskInput deploymentTaskInput)
        {
            if (deploymentTaskInput == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(deploymentTaskInput.PackageId))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(deploymentTaskInput.Version))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (!SemanticVersion.TryParse(deploymentTaskInput.Version, out SemanticVersion semanticVersion))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(deploymentTaskInput.TargetId))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            PackageVersion packageVersion = new PackageVersion(deploymentTaskInput.PackageId, semanticVersion);

            var deploymentTask = new DeploymentTask(packageVersion,
                deploymentTaskInput.TargetId,
                Guid.NewGuid(),
                User?.Identity?.Name);

            try
            {
                _deploymentService.Enqueue(deploymentTask);

                return RedirectToAction(nameof(Status), new { deploymentTask.DeploymentTargetId });
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Could not finish deploy of task {DeploymentTask}", deploymentTask);

                return new ContentResult
                {
                    Content = ex.ToString(),
                    StatusCode = 500
                };
            }
        }

        [HttpGet]
        [Route(DeploymentConstants.DeploymentStatusRoute, Name = DeploymentConstants.DeploymentStatusRouteName)]
        public IActionResult Status(string deploymentTargetId)
        {
            return View(new StatusViewOutputModel(deploymentTargetId));
        }
    }
}
