using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    public static class CustomWebHostExtensions
    {
        public static void CustomRunAsService(this IWebHost webHost)
        {
            var webHostService = new CustomWebHostService(webHost);

            ServiceBase.Run(webHostService);
        }
    }
}
