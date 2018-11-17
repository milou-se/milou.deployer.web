using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.IisHost.AspNetCore;

namespace Milou.Deployer.Web.IisHost.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [HttpGet]
        [Route(ErrorRouteConstants.ErrorRoute, Name = ErrorRouteConstants.ErrorRouteName)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));
        }
    }
}