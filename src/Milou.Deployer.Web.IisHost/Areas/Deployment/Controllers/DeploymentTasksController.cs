using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    [AllowAnonymous]
    public class DeploymentTasksController : BaseApiController
    {
        [Route("/deployment-tasks/{deploymentTaskId}")]
        public async Task<IActionResult> Index(string deploymentTaskId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("/deployment-task-package/{deploymentTaskId}")]
        public async Task<IActionResult> DeploymentTaskPackage([NotNull] string deploymentTaskId, [FromServices] IDeploymentTaskPackageStore deploymentTaskPackageStore)
        {
            if (string.IsNullOrWhiteSpace(deploymentTaskId))
            {
                return new BadRequestResult();
            }

            DeploymentTaskPackage deploymentTaskPackage =  await
                deploymentTaskPackageStore.GetDeploymentTaskPackageAsync(deploymentTaskId, CancellationToken.None);

            return new ObjectResult(deploymentTaskPackage);
        }
    }

}