using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks.Controllers
{
    [Area(WebHookConstants.AreaName)]
    [Route("hook")]
    public class HookController : BaseApiController
    {
        private readonly HookService _hookService;

        public HookController(HookService hookService)
        {
            _hookService = hookService;
        }

        [Route("")]
        [HttpPost]
        public async Task<HttpResponseMessage> Hook(IEnumerable<PackageVersion> packageIdentifiers)
        {
            await _hookService.AutoDeployAsync(packageIdentifiers.SafeToReadOnlyCollection());

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
