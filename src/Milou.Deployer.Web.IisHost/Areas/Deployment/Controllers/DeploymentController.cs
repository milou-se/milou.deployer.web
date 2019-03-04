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
using Milou.Deployer.Web.IisHost.Areas.Targets.Controllers;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [Area(DeploymentConstants.AreaName)]
    public class DeploymentController : BaseApiController
    {
        private readonly IDeploymentTargetReadService _getTargets;

        [NotNull]
        private readonly ILogger _logger;

        public DeploymentController(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetReadService getTargets)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getTargets = getTargets ?? throw new ArgumentNullException(nameof(getTargets));
        }

        [HttpGet]
        [Route("/deployment")]
        public async Task<IActionResult> Index(string prefix = null)
        {
            IReadOnlyCollection<DeploymentTarget> targets;
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    targets =
                        (await _getTargets.GetOrganizationsAsync(cts.Token)).SelectMany(
                            organization => organization.Projects.SelectMany(project => project.DeploymentTargets))
                        .SafeToReadOnlyCollection();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not get organizations");
                targets = ImmutableArray<DeploymentTarget>.Empty;
            }

            IReadOnlyCollection<PackageVersion> items = ImmutableArray<PackageVersion>.Empty;

            return View(new DeploymentViewOutputModel(items, targets));
        }

        [HttpGet]
        [Route(TargetConstants.InvalidateCacheRoute, Name = TargetConstants.InvalidateCacheRouteName)]
        public ActionResult InvalidateCache([FromServices] ICustomMemoryCache customMemoryCache)
        {
            customMemoryCache.Invalidate(Request.Query["prefix"]);

            return RedirectToAction(nameof(Index));
        }
    }
}
