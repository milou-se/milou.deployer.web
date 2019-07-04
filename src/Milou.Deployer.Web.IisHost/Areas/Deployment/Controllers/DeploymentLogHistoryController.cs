using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog.Events;

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
        public async Task<IActionResult> Log()
        {
            return View(new DeploymentLogViewOutputModel(ImmutableArray<LogItem>.Empty));
        }

        [Route(DeploymentConstants.HistoryLogRoute + ".json", Name = DeploymentConstants.HistoryLogRouteName + "Json")]
        [HttpGet]
        public async Task<ActionResult<string[]>> LogJson(
            [FromServices] IMediator mediator,
            [FromRoute] string deploymentTaskId,
            [FromQuery] string level = null)
        {
            var usedLevel = level.ParseOrDefault();

            var response = await mediator.Send(new DeploymentLogRequest(deploymentTaskId, usedLevel));

            return response.LogItems.Select(line => line.Message).ToArray();
        }
    }
}