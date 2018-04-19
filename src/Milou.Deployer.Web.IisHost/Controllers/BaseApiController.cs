using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.IisHost.Areas.Security;

namespace Milou.Deployer.Web.IisHost.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.IPOrToken)]
    public abstract class BaseApiController : Controller
    {
    }
}