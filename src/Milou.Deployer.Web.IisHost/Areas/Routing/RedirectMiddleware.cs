using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Milou.Deployer.Web.Core.Application;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Routing
{
    [UsedImplicitly]
    public class RedirectMiddleware
    {
        private const string LocationHeader = "location";
        private readonly EnvironmentConfiguration _environmentConfiguration;
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public RedirectMiddleware(
            EnvironmentConfiguration environmentConfiguration,
            ILogger logger,
            RequestDelegate next)
        {
            _environmentConfiguration = environmentConfiguration;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Could not make external request");
                throw;
            }

            if (context.Response.StatusCode == 302)
            {
                if (context.Response.Headers.TryGetValue(LocationHeader, out var values))
                {
                    if (string.IsNullOrWhiteSpace(_environmentConfiguration.PublicHostname))
                    {
                        return;
                    }

                    var uri = new Uri(values, UriKind.RelativeOrAbsolute);

                    if (!uri.IsAbsoluteUri)
                    {
                        return;
                    }

                    var builder = new UriBuilder(uri)
                    {
                        Host = _environmentConfiguration.PublicHostname
                    };

                    if (_environmentConfiguration.PublicPort.HasValue)
                    {
                        builder.Port = _environmentConfiguration.PublicPort.Value;
                    }

                    if (_environmentConfiguration.PublicPortIsHttps)
                    {
                        builder.Scheme = "https";
                    }

                    context.Response.Headers.Remove(LocationHeader);
                    context.Response.Headers.Add(LocationHeader, builder.Uri.ToString());
                }
            }
        }
    }
}
