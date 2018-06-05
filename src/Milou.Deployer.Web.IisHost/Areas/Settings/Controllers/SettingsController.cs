using System;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    [Area(SettingsConstants.AreaName)]
    [Route("settings")]
    public class SettingsController : BaseApiController
    {
        public const string BaseRoute = "settings";

        [HttpGet]
        [Route("")]
        public IActionResult Index([FromServices] IDeploymentTargetReadService deploymentTargetReadService)
        {
            return View(new SettingsViewModel(deploymentTargetReadService.GetType().Name,
                RouteList.GetConstantRoutes(AppDomain.CurrentDomain.FilteredAssemblies())));
        }
    }
}