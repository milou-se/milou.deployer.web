using System;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Extensions.BoolExtensions;
using Arbor.KVConfiguration.Core.Extensions.IntExtensions;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public class CustomRequireHttpsAttribute : RequireHttpsAttribute
    {
        protected override void HandleNonHttpsRequest(AuthorizationFilterContext filterContext)
        {
            base.HandleNonHttpsRequest(filterContext);

            bool httpsEnabled =
                StaticKeyValueConfigurationManager.AppSettings.ValueOrDefault("milou-deployer-web:https:enabled", true);

            if (!httpsEnabled)
            {
                return;
            }

            if (!string.Equals(filterContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("https is required");
            }

            Uri uri = filterContext.HttpContext.Request.GetUri();
            if (uri == null)
            {
                throw new InvalidOperationException("The current URL is null");
            }

            var uriBuilder = new UriBuilder(uri);

            int httpsPort =
                StaticKeyValueConfigurationManager.AppSettings.ValueOrDefault(
                    "milou-deployer-web:https:custom-port",
                    443);

            if (httpsPort <= 0)
            {
                throw new InvalidOperationException($"Invalid https port specified: {httpsPort}");
            }

            uriBuilder.Scheme = "https";
            uriBuilder.Port = httpsPort;

            string url = uriBuilder.Uri.ToString();

            filterContext.Result = new RedirectResult(url);
        }
    }
}