﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Arbor.App.Extensions.Application;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
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
            catch (Exception ex)
            {
                if (ex.Message.Contains("An error was encountered while handling the remote login.", StringComparison.OrdinalIgnoreCase))
                {
                    await context.SignOutAsync();
                    context.Response.Redirect("/");
                    return;
                }

                throw;
            }

            if (context.Response.StatusCode == 302)
            {
                if (context.Response.Headers.TryGetValue(LocationHeader, out var values))
                {
                    if (values.Count == 1 && values[0].StartsWith("/"))
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(_environmentConfiguration.PublicHostname))
                    {
                        return;
                    }

                    if (!_environmentConfiguration.PublicHostname.Equals(values, StringComparison.OrdinalIgnoreCase))
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

                    if (_environmentConfiguration.PublicPortIsHttps == true)
                    {
                        builder.Scheme = "https";
                    }
                    else if (_environmentConfiguration.PublicPortIsHttps == false)
                    {
                        builder.Scheme = "http";
                    }

                    context.Response.Headers.Remove(LocationHeader);
                    context.Response.Headers.Add(LocationHeader, builder.Uri.ToString());
                }
            }
        }
    }
}
