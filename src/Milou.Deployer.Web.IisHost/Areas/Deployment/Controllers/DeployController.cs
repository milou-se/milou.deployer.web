using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewInputModels;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    public class DeployController : BaseApiController
    {
        private readonly DeploymentWorker _deploymentService;

        private readonly ILogger _logger;

        public DeployController(ILogger logger, DeploymentWorker deploymentService)
        {
            _logger = logger;
            _deploymentService = deploymentService;
        }

        [Route(DeploymentConstants.DeployRoute,Name= DeploymentConstants.DeployRouteName)]
        [HttpPost]
        public IActionResult Index(
            DeploymentTaskInput deploymentTaskInput)
        {
            if (deploymentTaskInput == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(deploymentTaskInput.PackageVersion))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(deploymentTaskInput.TargetId))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var deploymentTask = new DeploymentTask(deploymentTaskInput.PackageVersion,
                deploymentTaskInput.TargetId,
                Guid.NewGuid());

            try
            {
                _deploymentService.Enqueue(deploymentTask);

                return RedirectToAction(nameof(Status), new { deploymentTask.DeploymentTargetId });
            }
            catch (Exception ex)
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
        [Route(DeploymentConstants.DeploymentStatusRoute, Name=DeploymentConstants.DeploymentStatusRouteName)]
        public IActionResult Status(string deploymentTargetId)
        {
            return View(new StatusViewOutputModel(deploymentTargetId));
        }
    }
}