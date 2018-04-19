using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [Route(NugetServerBaseRoute)]
    public class NuGetServerController : BaseApiController
    {
        public const string NugetServerBaseRoute = "nugetserver";

        public const string ClearPostRoute = "clear";

        private readonly NuGetService _nugetService;

        public NuGetServerController(NuGetService nugetService)
        {
            _nugetService = nugetService;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route(ClearPostRoute)]
        public async Task<ActionResult> ClearCache()
        {
            await _nugetService.ClearAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}