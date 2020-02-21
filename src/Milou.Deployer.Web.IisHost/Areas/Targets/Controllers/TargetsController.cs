﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Targets.ViewModels;
using Milou.Deployer.Web.IisHost.Areas.Targets.Views;
using Milou.Deployer.Web.IisHost.AspNetCore.TempData;
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
        [Route(TargetConstants.RemoveTargetPostRoute, Name = TargetConstants.RemoveTargetPostRouteName)]
        public async Task<IActionResult> Remove([FromBody] RemoveTarget removeTarget, [FromServices] IMediator mediator)
        {
            if (removeTarget is null)
            {
                return BadRequest($"Model of type {typeof(RemoveTarget)} is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await mediator.Send(removeTarget);

            return RedirectToRoute(MonitorConstants.MonitorRouteName);
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

            CreateTargetResult createTargetResult = await mediator.Send(createTarget);

            if (redirect)
            {
                TempData.Put(createTargetResult);

                return RedirectToRoute(MonitorConstants.MonitorRouteName);
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
            [FromRoute] string deploymentTargetId,
            [FromServices] IDeploymentTargetReadService deploymentTargetReadService,
            [FromServices] IEnvironmentTypeService environmentTypeService)
        {
            var deploymentTarget = await deploymentTargetReadService.GetDeploymentTargetAsync(deploymentTargetId);

            if (deploymentTarget is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var environmentTypes = await environmentTypeService.GetEnvironmentTypes();

            return View(new EditTargetViewOutputModel(deploymentTarget, environmentTypes));
        }

        [AllowAnonymous]
        [Route(TargetConstants.TargetRoute, Name = TargetConstants.TargetRouteName)]
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromRoute] string deploymentTargetId,
            [FromServices] IDeploymentTargetReadService deploymentTargetReadService,
            [FromServices] IEnvironmentTypeService environmentTypeService)
        {
            var deploymentTarget = await deploymentTargetReadService.GetDeploymentTargetAsync(deploymentTargetId);

            if (deploymentTarget is null)
            {
                return new NotFoundResult();
            }

            return new ObjectResult(deploymentTarget);
        }

        [Route(TargetConstants.EditTargetPostRoute, Name = TargetConstants.EditTargetPostRouteName)]
        [HttpPost]
        public async Task<ActionResult<UpdateDeploymentTargetResult>> Edit(
            [FromBody] UpdateDeploymentTarget updateDeploymentTarget,
            [FromServices] IMediator mediator,
            [FromQuery] bool redirect = true)
        {
            if (updateDeploymentTarget is null)
            {
                return BadRequest($"Model of type {typeof(UpdateDeploymentTarget)} is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!updateDeploymentTarget.IsValid)
            {
                return BadRequest($"Model of type {typeof(UpdateDeploymentTarget)} {updateDeploymentTarget} is null");
            }

            UpdateDeploymentTargetResult updateDeploymentTargetResult = await mediator.Send(updateDeploymentTarget);

            if (redirect)
            {
                TempData.Put<ITargetResult>(updateDeploymentTargetResult);

                return RedirectToRoute(MonitorConstants.MonitorRouteName);
            }

            return updateDeploymentTargetResult;
        }

        [Route(TargetConstants.EnableTargetPostRoute, Name = TargetConstants.EnableTargetPostRouteName)]
        [HttpPost]
        public async Task<IActionResult> Enable(
            [FromBody] EnableTarget enableTarget,
            [FromServices] IMediator mediator)
        {
            await mediator.Send(enableTarget);

            return Redirect("/");
        }

        [Route(TargetConstants.DisableTargetPostRoute, Name = TargetConstants.DisableTargetPostRouteName)]
        [HttpPost]
        public async Task<IActionResult> Disable(
            [FromBody] DisableTarget disableTarget,
            [FromServices] IMediator mediator)
        {
            await mediator.Send(disableTarget);

            return Redirect("/");
        }

        [Route(TargetConstants.DisabledTargetsRoute, Name = TargetConstants.DisabledTargetsRouteName)]
        [HttpGet]
        public async Task<IActionResult> Disabled(
            [FromServices] IDeploymentTargetReadService deploymentTargetReadService)
        {
            var targets =
                await deploymentTargetReadService.GetDeploymentTargetsAsync(new TargetOptions { OnlyEnabled = false });

            return View(new TargetListViewModel(targets.OrderBy(target => target.Enabled).ToImmutableArray()));
        }
    }
}
