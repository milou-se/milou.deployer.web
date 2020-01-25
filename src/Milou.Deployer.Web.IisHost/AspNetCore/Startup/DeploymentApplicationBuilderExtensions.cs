using System.Collections.Generic;
using System.IO;
using Arbor.App.Extensions.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;
using Milou.Deployer.Web.IisHost.Areas.ErrorHandling;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
{
    public static class DeploymentApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddExceptionHandling(
            this IApplicationBuilder app,
            EnvironmentConfiguration environmentConfiguration)
        {
            if (environmentConfiguration.UseVerboseExceptions)
            {
                return app.UseDeveloperExceptionPage();
            }

            return app.UseExceptionHandler(ErrorRouteConstants.ErrorRoute);
        }

        public static IApplicationBuilder AddForwardHeaders(
            this IApplicationBuilder app,
            EnvironmentConfiguration environmentConfiguration)
        {
            var forwardedHeadersOptions =
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };

            foreach (var proxyAddress in environmentConfiguration.ProxyAddresses)
            {
                forwardedHeadersOptions.KnownProxies.Add(proxyAddress);
            }

            if (environmentConfiguration.ForwardLimit.HasValue)
            {
                forwardedHeadersOptions.ForwardLimit = environmentConfiguration.ForwardLimit.Value;
            }

            return app.UseForwardedHeaders(forwardedHeadersOptions);
        }

        public static IApplicationBuilder UseCustomStaticFiles(
            this IApplicationBuilder app,
            EnvironmentConfiguration environmentConfiguration)
        {
            string wwwrootPath = Path.Combine(environmentConfiguration.ContentBasePath, "wwwroot");
            var providers = new List<IFileProvider>
                            {
                                new PhysicalFileProvider(wwwrootPath)
                            };

            string contentPath = Path.Combine(wwwrootPath, "_content", "Milou.Deployer.Web.IisHost");

            if (Directory.Exists(contentPath))
            {
                providers.Add(new PhysicalFileProvider(contentPath));
            }

            var staticFileOptions = new StaticFileOptions { FileProvider = new CompositeFileProvider(providers) };

            return app.UseStaticFiles(staticFileOptions);
        }

        public static void UseSignalRHubs(this IEndpointRouteBuilder routerBuilder)
        {
            routerBuilder.MapHub<TargetHub>(DeploymentLogConstants.HubRoute);
        }
    }
}