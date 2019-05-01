using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    [Area(SettingsConstants.AreaName)]
    public class SettingsController : BaseApiController
    {
        public const string BaseRoute = "settings";

        [HttpGet]
        [Route(SettingsConstants.SettingsGetRoute, Name = SettingsConstants.SettingsGetRouteName)]
        public async Task<ActionResult<SettingsViewModel>> Index(
            [FromServices] MultiSourceKeyValueConfiguration configuration,
            [FromServices] IMediator mediator)
        {
            if (!configuration[SettingsConstants.DiagnosticsEnabled].ParseAsBooleanOrDefault())
            {
                return new StatusCodeResult(403);
            }

            var settingsViewModel = await mediator.Send(new SettingsViewRequest());

            return View(settingsViewModel);
        }

        [HttpPost]
        [Route(SettingsConstants.LogSettingsPostRoute, Name = SettingsConstants.LogSettingsPostRouteName)]
        public async Task<IActionResult> LogLevel(
            [FromBody] ChangeLogLevel changeLogLevel,
            [FromServices] IMediator mediator)
        {
            await mediator.Send(new ChangeLogLevelRequest(changeLogLevel));

            return RedirectToAction(nameof(Index));
        }
    }
}
