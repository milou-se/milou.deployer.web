using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class CustomWebHostService : WebHostService
    {
        private readonly App _app;

        public CustomWebHostService(IWebHost webHost, App app) : base(webHost)
        {
            _app = app;
        }

        protected override void OnStarted()
        {
            _app.Logger.Debug("Scope diagnostics {Diagnostics}", _app.AppRootScope.Top().Diagnostics());
        }
    }
}