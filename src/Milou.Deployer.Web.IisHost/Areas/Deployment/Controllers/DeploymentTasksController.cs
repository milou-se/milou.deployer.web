using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    public class DeploymentTasksController : AgentApiController
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public DeploymentTasksController(ILogger logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        [Route("/deployment-tasks/{deploymentTaskId}")]
        public async Task<IActionResult> Index(string deploymentTaskId) => throw new NotImplementedException();

        [HttpGet]
        [Route("/deployment-task-package/{deploymentTaskId}")]
        public async Task<IActionResult> DeploymentTaskPackage([NotNull] string deploymentTaskId,
            [FromServices] IDeploymentTaskPackageStore deploymentTaskPackageStore)
        {
            if (string.IsNullOrWhiteSpace(deploymentTaskId))
            {
                return new BadRequestResult();
            }

            DeploymentTaskPackage deploymentTaskPackage = await
                deploymentTaskPackageStore.GetDeploymentTaskPackageAsync(deploymentTaskId, CancellationToken.None);

            return new ObjectResult(deploymentTaskPackage);
        }

        [Route("/deployment-task/result")]
        [HttpPost]
        public async Task<IActionResult> DeployAgentResult(DeploymentTaskAgentResult deploymentTaskAgentResult)
        {
            //TODO check deploymentTargetId and deploymentTaskId belongs together

            if (deploymentTaskAgentResult.Succeeded)
            {
                _logger.Information("Deploy succeeded for deployment task id {DeploymentTaskId}",
                    deploymentTaskAgentResult.DeploymentTaskId);

                await _mediator.Publish(new AgentDeploymentDoneNotification(deploymentTaskAgentResult.DeploymentTaskId,
                    deploymentTaskAgentResult.DeploymentTargetId));
            }
            else
            {
                _logger.Error("Deploy failed for deployment task id {DeploymentTaskId}",
                    deploymentTaskAgentResult.DeploymentTaskId);

                await _mediator.Publish(new AgentDeploymentFailedNotification(
                    deploymentTaskAgentResult.DeploymentTaskId, deploymentTaskAgentResult.DeploymentTargetId));
            }

            return Ok();
        }
    }
}