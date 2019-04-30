using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public sealed class WebHostWrapper : IWebHost
    {
        private readonly IWebHost _webHostImplementation;

        public WebHostWrapper([NotNull] IWebHost webHost)
        {
            _webHostImplementation = webHost ?? throw new ArgumentNullException(nameof(webHost));
        }

        public void Dispose()
        {
            _webHostImplementation.Dispose();
        }

        public void Start()
        {
            _webHostImplementation.Start();
        }

        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var applicationLifetime = _webHostImplementation.Services.GetService<IApplicationLifetime>();

            var logger = _webHostImplementation.Services.GetService<ILogger>();

            return _webHostImplementation.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _webHostImplementation.StopAsync(cancellationToken);
        }

        public IFeatureCollection ServerFeatures => _webHostImplementation.ServerFeatures;

        public IServiceProvider Services => _webHostImplementation.Services;
    }
}
