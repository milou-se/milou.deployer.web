using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class RedirectMiddleware
    {
        private const string LocationHeader = "location";
        private readonly EnvironmentConfiguration _environmentConfiguration;
        private readonly RequestDelegate _next;

        public RedirectMiddleware(
            EnvironmentConfiguration environmentConfiguration,
            RequestDelegate next)
        {
            _environmentConfiguration = environmentConfiguration;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

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

                    if (!uri.Host.Equals(context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
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
