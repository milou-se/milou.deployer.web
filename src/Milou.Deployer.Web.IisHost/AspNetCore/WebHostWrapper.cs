using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Configuration;
using Serilog;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public sealed class WebHostWrapper : IWebHost
    {
        private IWebHost _webHostImplementation;
        private Scope _scope;

        public WebHostWrapper([NotNull] IWebHost webHost, Scope scope)
        {
            _webHostImplementation = webHost ?? throw new ArgumentNullException(nameof(webHost));
            _scope = scope;
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

            var logger = _scope.Lifetime.Resolve<ILogger>();

            applicationLifetime.ApplicationStarted.Register(() =>
                logger.Debug("Scope chain {Scopes}", _scope.Top().Diagnostics()));

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