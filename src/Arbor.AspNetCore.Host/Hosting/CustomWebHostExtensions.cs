using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

namespace Arbor.AspNetCore.Host.Hosting
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
