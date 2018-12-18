using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public class LoginController : Controller
    {
        [AllowAnonymous]
        [Route("/login")]
        // GET
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("/login/external")]
        public IActionResult MakeChallenge()
        {
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        [Route("/me")]
        public IActionResult Me()
        {
            return new ObjectResult(
                new {Claims = HttpContext.User.Claims.Select(c => c.Type + " " + c.Value).ToArray()});
        }
    }
}