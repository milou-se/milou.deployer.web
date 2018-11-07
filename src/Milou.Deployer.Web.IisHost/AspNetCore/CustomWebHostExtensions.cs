using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public static class CustomWebHostExtensions
    {
        public static void CustomRunAsService(this IWebHost webHost, App app)
        {
            var webHostService = new CustomWebHostService(webHost, app);

            ServiceBase.Run(webHostService);
        }
    }
}