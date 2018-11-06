using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class CustomWebHostService : WebHostService
    {
        private readonly App _app;

        public CustomWebHostService(
            [NotNull] IWebHost webHost,
            [NotNull] App app) : base(webHost)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        protected override void OnStarted()
        {
            if (_app.Logger.IsEnabled(LogEventLevel.Debug))
            {
                _app.Logger.Debug("Scope diagnostics {Diagnostics}", _app.AppRootScope.Top().Diagnostics());
            }
        }
    }
}