using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Serilog;

namespace Milou.Deployer.Web.Core.Http
{
    public sealed class CustomHttpClientFactory : IHttpClientFactory, IDisposable
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _logger;

        public CustomHttpClientFactory([NotNull] ILifetimeScope scope, [NotNull] ILogger logger)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool _isDisposed;

        private static readonly ConcurrentDictionary<string, HttpClient> _Clients = new ConcurrentDictionary<string, HttpClient>(StringComparer.OrdinalIgnoreCase);

        public HttpClient CreateClient(string name)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException($"Could not create HttpClient for name '{name}', {nameof(CustomHttpClientFactory)} has been disposed");
            }

            if (_scope.TryResolve(out Scope scope))
            {
                ILifetimeScope lifetimeScope = scope.Deepest().Lifetime;

                if (lifetimeScope.TryResolve(out IHttpClientFactory factory))
                {
                    return factory.CreateClient(name);
                }
            }

            if (_scope.TryResolve(out IHttpClientFactory scopeFactory))
            {
                return scopeFactory.CreateClient(name);
            }

            return GetOrCreateClient(string.IsNullOrWhiteSpace(name) ? "" : name);
        }

        private HttpClient GetOrCreateClient(string name)
        {
            if (_Clients.TryGetValue(name, out HttpClient httpClient))
            {
                return httpClient;
            }

            var client = new HttpClient();

            if (!_Clients.TryAdd(name, client))
            {
                client.Dispose();
            }

            return _Clients[name];
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            foreach (KeyValuePair<string, HttpClient> keyValuePair in _Clients)
            {
                keyValuePair.Value.Dispose();
                _logger.Verbose("Disposed Http client named {Name}", keyValuePair.Key);
            }

            _Clients.Clear();
            _isDisposed = true;
        }
    }
}