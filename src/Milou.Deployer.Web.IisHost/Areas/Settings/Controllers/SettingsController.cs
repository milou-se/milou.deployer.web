using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Core.Extensions;
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
        public async Task<ActionResult<SettingsViewModel>> Index(
            [FromServices] MultiSourceKeyValueConfiguration configuration,
            [FromServices] IMediator mediator)
        {
            if (!configuration[SettingsConstants.DiagnosticsEnabled].ParseAsBooleanOrDefault())
            {
                return new StatusCodeResult(403);
            }

            SettingsViewModel settingsViewModel = await mediator.Send(new SettingsViewRequest());

            return View(settingsViewModel);
        }
    }
}