using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    public class CustomWebHostService : WebHostService
    {
        public CustomWebHostService(
            [NotNull] IWebHost webHost) : base(webHost)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }
        }
    }
}
