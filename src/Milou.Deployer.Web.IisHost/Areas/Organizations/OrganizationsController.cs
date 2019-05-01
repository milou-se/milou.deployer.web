using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.IisHost.AspNetCore.TempData;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Organizations
{
    [Area(OrganizationConstants.AreaName)]
    public class OrganizationsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route(OrganizationConstants.OrganizationBaseRoute, Name = OrganizationConstants.OrganizationBaseRouteName)]
        [HttpGet]
        public async Task<IActionResult> Index([FromServices] IDeploymentTargetReadService deploymentTargetReadService)
        {
            var organizations = await deploymentTargetReadService.GetOrganizationsAsync();

            var createOrganizationResult = TempData.Get<CreateOrganizationResult>();

            return View(new OrganizationsViewOutputModel(organizations, createOrganizationResult));
        }

        [HttpPost]
        [Route(OrganizationConstants.CreateOrganizationPostRoute,
            Name = OrganizationConstants.CreateOrganizationPostRouteName)]
        public async Task<ActionResult<CreateOrganizationResult>> Post(
            [FromBody] CreateOrganization createOrganization,
            [FromQuery] bool redirect = true)
        {
            var createOrganizationResult = await _mediator.Send(createOrganization);

            if (redirect)
            {
                TempData.Put(createOrganizationResult);
                return RedirectToAction(nameof(Index));
            }

            return createOrganizationResult;
        }
    }
}
