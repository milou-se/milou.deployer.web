using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    [Route("deployment")]
    public class DeploymentController : BaseApiController
    {
        private readonly DeploymentService _deploymentService;

        private readonly IDeploymentTargetReadService _getTargets;

        public DeploymentController(
            [NotNull] ILogger logger,
            [NotNull] DeploymentService deploymentService,
            [NotNull] IDeploymentTargetReadService getTargets)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _getTargets = getTargets ?? throw new ArgumentNullException(nameof(getTargets));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(string prefix = null)
        {
            IReadOnlyCollection<DeploymentTarget> targets =
                (await _getTargets.GetOrganizationsAsync(CancellationToken.None)).SelectMany(
                    organization => organization.Projects.SelectMany(project => project.DeploymentTargets))

                .SafeToReadOnlyCollection();

            ImmutableArray<string> allAllowedPackageNames =
                targets.Select(target => target.PackageId).ToImmutableArray();

            IReadOnlyCollection<PackageVersion> items = ImmutableArray<PackageVersion>.Empty;

            return View(new DeploymentViewOutputModel(items, targets));
        }

        [HttpGet]
        [Route("invalidate")]
        public async Task<ActionResult> InvalidateCache([FromServices] ICustomMemoryCache customMemoryCache)
        {
            customMemoryCache.Invalidate();

            return RedirectToAction(nameof(Index));
        }
    }
}