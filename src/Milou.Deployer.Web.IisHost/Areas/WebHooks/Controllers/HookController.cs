using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks.Controllers
{
    [Area(WebHookConstants.AreaName)]
    [Route("hook")]
    public class HookController : BaseApiController
    {
        private readonly PackageWebHookHandler _packageWebHookHandler;

        public HookController(PackageWebHookHandler packageWebHookHandler) =>
            _packageWebHookHandler = packageWebHookHandler;

        [Route("")]
        [HttpPost]
        public async Task<HttpResponseMessage> Hook()
        {
            await _packageWebHookHandler.HandleRequest(Request);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}