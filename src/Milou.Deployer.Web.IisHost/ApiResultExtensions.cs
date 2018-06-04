using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost
{
    public static class ApiResultExtensions
    {
        public static IActionResult ToActionResult(this ApiResult apiResult)
        {
            return new ObjectResult(apiResult);
        }
    }
}