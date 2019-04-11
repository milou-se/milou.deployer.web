using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Targets;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.Controllers
{
    [Area(TargetConstants.AreaName)]
    public class TargetsController : BaseApiController
    {
        private readonly IDeploymentTargetReadService _targetSource;

        public TargetsController([NotNull] IDeploymentTargetReadService targetSource)
        {
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
        }

        [HttpGet]
        [Route(TargetConstants.TargetsRoute, Name = TargetConstants.TargetsRouteName)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<OrganizationInfo> organizations =
                await _targetSource.GetOrganizationsAsync(cancellationToken);

            var targetsViewModel = new OrganizationsViewModel(organizations);

            return View(targetsViewModel);
        }

        [HttpPost]
        [Route(TargetConstants.CreateTargetPostRoute, Name = TargetConstants.CreateTargetPostRouteName)]
        public async Task<ActionResult<CreateTargetResult>> Post(
            [FromBody] CreateTarget createTarget,
            [FromServices] IMediator mediator,
            [FromQuery] bool redirect = true)
        {
            if (createTarget is null)
            {
                return BadRequest($"Model of type {typeof(CreateTarget)} is null");
            }

            if (!createTarget.IsValid)
            {
                return BadRequest($"Model of type {typeof(CreateTarget)} {createTarget} is invalid");
            }

            var createTargetResult = await mediator.Send(createTarget);

            if (redirect)
            {
                //TempData.Put(createTargetResult);

                //if (createTargetResult.TargetName.IsNullOrWhiteSpace())
                //{
                //    return new RedirectToRouteResult(OrganizationConstants.OrganizationBaseRouteName);
                //}

                //return RedirectToRoute(ProjectConstants.ProjectsBaseRouteName,
                //    new { organizationId = createTargetResult.OrganizationId });
            }

            return createTargetResult;
        }

        [Route(TargetConstants.CreateTargetGetRoute, Name = TargetConstants.CreateTargetGetRouteName)]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTargetViewOutputModel());
        }

        [Route(TargetConstants.EditTargetRoute, Name = TargetConstants.EditTargetRouteName)]
        [HttpGet]
        public async Task<IActionResult> Edit(
            [FromRoute] string targetId,
            [FromServices] IDeploymentTargetReadService deploymentTargetReadService)
        {
            var deploymentTarget = await deploymentTargetReadService.GetDeploymentTargetAsync(targetId);

            if (deploymentTarget is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new EditTargetViewOutputModel(deploymentTarget));
        }

        [Route(TargetConstants.EditTargetPostRoute, Name = TargetConstants.EditTargetPostRouteName)]
        [HttpPost]
        public async Task<ActionResult<UpdateDeploymentTargetResult>> Edit(
            [FromBody] UpdateDeploymentTarget updateDeploymentTarget,
            [FromServices] IMediator mediator)
        {
            if (updateDeploymentTarget is null)
            {
                return BadRequest($"Model of type {typeof(UpdateDeploymentTarget)} is null");
            }

            if (!updateDeploymentTarget.IsValid)
            {
                return BadRequest($"Model of type {typeof(UpdateDeploymentTarget)} {updateDeploymentTarget} is null");
            }

            var updateDeploymentTargetResult = await mediator.Send(updateDeploymentTarget);

            return updateDeploymentTargetResult;
        }
    }
}
