using System;
using System.Collections.Immutable;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Deployment;
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
        public IActionResult Index([FromServices] IDeploymentTargetReadService deploymentTargetReadService, [FromServices] MultiSourceKeyValueConfiguration configuration)
        {
            if (!configuration[SettingsConstants.DiagnosticsEnabled].ParseAsBooleanOrDefault())
            {
                return new StatusCodeResult(403);
            }

            ImmutableArray<ControllerRouteInfo> routesWithController = RouteList.GetRoutesWithController(AppDomain.CurrentDomain.FilteredAssemblies());

            var info = new ConfigurationInfo(configuration.SourceChain, configuration.AllKeys.OrderBy(item => item).Select(item => new ConfigurationKeyInfo(item, configuration[item], configuration.ConfiguratorFor(item).GetType().Name)).ToImmutableArray());

            var settingsViewModel = new SettingsViewModel(
                deploymentTargetReadService.GetType().Name,
                routesWithController, info);

            return View(settingsViewModel);
        }
    }
}