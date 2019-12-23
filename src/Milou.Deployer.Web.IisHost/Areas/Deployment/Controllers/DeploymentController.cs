using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Time;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Caching;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Extensions;
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

        private readonly TimeoutHelper _timeoutHelper;

        public DeploymentController(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetReadService getTargets,
            TimeoutHelper timeoutHelper)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getTargets = getTargets ?? throw new ArgumentNullException(nameof(getTargets));
            _timeoutHelper = timeoutHelper;
        }

        [HttpGet]
        [Route("/deployment")]
        public async Task<IActionResult> Index()
        {
            IReadOnlyCollection<DeploymentTarget> targets;
            try
            {
                using (var cts = _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(30)))
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
