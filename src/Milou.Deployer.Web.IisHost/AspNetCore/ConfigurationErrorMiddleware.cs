using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class ConfigurationErrorMiddleware
    {
        private readonly ImmutableArray<ConfigurationError> _configurationErrors;
        private readonly RequestDelegate _next;

        public ConfigurationErrorMiddleware(
            IEnumerable<ConfigurationError> configurationErrors,
            RequestDelegate next)
        {
            _configurationErrors = configurationErrors.ToImmutableArray();
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_configurationErrors.Length > 0 &&
                !context.Request.Path.StartsWithSegments(ErrorRouteConstants.ErrorRoute,
                    StringComparison.OrdinalIgnoreCase))
            {
                var message = "Application configuration is invalid";

                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(message);

                foreach (var configurationError in _configurationErrors)
                {
                    stringBuilder.AppendLine(configurationError.Error);
                }

                throw new DeployerAppException(stringBuilder.ToString());
            }

            await _next(context);
        }
    }
}
