using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Structure;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.Controllers
{
    [Area(TargetConstants.AreaName)]
    [Route("targets")]
    public class TargetsController : BaseApiController
    {
        private readonly IDeploymentTargetReadService _targetSource;

        public TargetsController([NotNull] IDeploymentTargetReadService targetSource)
        {
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<OrganizationInfo> organizations = await _targetSource.GetOrganizationsAsync(cancellationToken);

            var targetsViewModel = new OrganizationsViewModel(organizations);

            return View(targetsViewModel);
        }
    }
}