using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Security;

namespace Milou.Deployer.Web.IisHost.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.IpOrToken)]
    public abstract class BaseApiController : Controller
    {
    }
}
