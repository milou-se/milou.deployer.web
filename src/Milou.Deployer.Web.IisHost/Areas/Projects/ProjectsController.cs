using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Structure;
using Milou.Deployer.Web.IisHost.Areas.Organizations;
using Milou.Deployer.Web.IisHost.AspNetCore;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Projects
{
    [ApiController]
    [Area(ProjectConstants.AreaName)]
    public class ProjectsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ProjectsController([NotNull] IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Route(ProjectConstants.ProjectsBaseRoute, Name = ProjectConstants.ProjectsBaseRouteName)]
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromServices] IDeploymentTargetReadService deploymentTargetReadService,
            [FromRoute] string organizationId)
        {
            ImmutableArray<ProjectInfo> projects = await deploymentTargetReadService.GetProjectsAsync(organizationId);

            var createProjectResult = TempData.Get<CreateProjectResult>();

            return View(new ProjectsViewOutputModel(projects, createProjectResult, organizationId));
        }

        [HttpPost]
        [Route(ProjectConstants.CreateProjectPostRoute,
            Name = ProjectConstants.CreateProjectPostRouteName)]
        public async Task<ActionResult<CreateProjectResult>> Post([FromBody] CreateProject createProject, [FromQuery] bool redirect = true)
        {
            CreateProjectResult createProjectResult = await _mediator.Send(createProject);

            if (redirect)
            {
                TempData.Put(createProjectResult);

                if (createProject.OrganizationId.IsNullOrWhiteSpace())
                {
                    return new RedirectToRouteResult(OrganizationConstants.OrganizationBaseRouteName);
                }

                return RedirectToRoute(ProjectConstants.ProjectsBaseRouteName,
                    new { organizationId = createProject.OrganizationId });
            }

            return createProjectResult;
        }
    }
}