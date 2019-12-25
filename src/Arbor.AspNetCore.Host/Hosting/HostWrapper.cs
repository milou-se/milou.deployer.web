using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Arbor.AspNetCore.Host.Hosting
{
    public sealed class HostWrapper : IHost
    {
        private readonly IHost _webHostImplementation;

        public HostWrapper([NotNull] IHost webHost) =>
            _webHostImplementation = webHost ?? throw new ArgumentNullException(nameof(webHost));

        public void Dispose() => _webHostImplementation.Dispose();

        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            _webHostImplementation.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            _webHostImplementation.StopAsync(cancellationToken);

        public IServiceProvider Services => _webHostImplementation.Services;

        public void Start() => _webHostImplementation.Start();
    }
}