using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    public class DeploymentLogHistoryController : BaseApiController
    {
        [Route(DeploymentConstants.HistoryRoute, Name = DeploymentConstants.HistoryRouteName)]
        [HttpGet]
        public async Task<IActionResult> Index([FromServices] IMediator mediator, [FromRoute] string deploymentTargetId)
        {
            var response = await mediator.Send(new DeploymentHistoryRequest(deploymentTargetId));

            return View(new DeploymentHistoryViewOutputModel(response.DeploymentTasks));
        }

        [Route(DeploymentConstants.HistoryLogRoute, Name = DeploymentConstants.HistoryLogRouteName)]
        [HttpGet]
        public async Task<IActionResult> Log(
            [FromServices] IMediator mediator,
            [FromRoute] string deploymentTaskId)
        {
            var response = await mediator.Send(new DeploymentLogRequest(deploymentTaskId));

            return View(new DeploymentLogViewOutputModel(response.Log));
        }
    }
}
