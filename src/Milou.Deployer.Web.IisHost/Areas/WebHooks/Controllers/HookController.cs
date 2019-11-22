using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks.Controllers
{
    [Area(WebHookConstants.AreaName)]
    [ApiController]
    public class HookController : Controller
    {
        private readonly PackageWebHookHandler _packageWebHookHandler;

        public HookController(PackageWebHookHandler packageWebHookHandler) =>
            _packageWebHookHandler = packageWebHookHandler;

        [AllowAnonymous]
        [Route("~/hook")]
        [HttpPost]
        public async Task<IActionResult> Hook()
        {
            string content;
            using (var streamReader = new StreamReader(Request.Body))
            {
                content = await streamReader.ReadToEndAsync();
            }

            var result = await _packageWebHookHandler.HandleRequest(Request, content);

            if (!result.Handled)
            {
                return BadRequest(new { Error = "Invalid web hook" });
            }

            return Ok();
        }
    }
}