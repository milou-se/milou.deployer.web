//using System;
//using System.Collections.Concurrent;
//using System.Net.Http;
//using JetBrains.Annotations;
//using Milou.Deployer.Web.Core.Configuration;
//using Serilog;

//namespace Milou.Deployer.Web.Core.Http
//{
//    public sealed class CustomHttpClientFactory : IHttpClientFactory, IDisposable
//    {
//        private static readonly ConcurrentDictionary<string, HttpClient> Clients =
//            new ConcurrentDictionary<string, HttpClient>(StringComparer.OrdinalIgnoreCase);

//        private readonly ILogger _logger;
//        private readonly ILifetimeScope _scope;

//        private bool _isDisposed;

//        public CustomHttpClientFactory([NotNull] ILifetimeScope scope, [NotNull] ILogger logger)
//        {
//            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        private HttpClient GetOrCreateClient(string name)
//        {
//            if (Clients.TryGetValue(name, out var httpClient))
//            {
//                return httpClient;
//            }

//            var client = new HttpClient();

//            if (!Clients.TryAdd(name, client))
//            {
//                client.Dispose();
//            }

//            return Clients[name];
//        }

//        public void Dispose()
//        {
//            if (_isDisposed)
//            {
//                return;
//            }

//            foreach (var keyValuePair in Clients)
//            {
//                keyValuePair.Value.Dispose();
//                _logger.Verbose("Disposed Http client named {Name}", keyValuePair.Key);
//            }

//            Clients.Clear();
//            _isDisposed = true;
//        }

//        public HttpClient CreateClient(string name)
//        {
//            if (_isDisposed)
//            {
//                throw new ObjectDisposedException(
//                    $"Could not create HttpClient for name '{name}', {nameof(CustomHttpClientFactory)} has been disposed");
//            }

//            if (_scope.TryResolve(out Scope scope))
//            {
//                var lifetimeScope = scope.Deepest().Lifetime;

//                if (lifetimeScope.TryResolve(out IHttpClientFactory factory))
//                {
//                    return factory.CreateClient(name);
//                }
//            }

//            if (_scope.TryResolve(out IHttpClientFactory scopeFactory))
//            {
//                return scopeFactory.CreateClient(name);
//            }

//            return GetOrCreateClient(string.IsNullOrWhiteSpace(name) ? "" : name);
//        }
//    }
//}
