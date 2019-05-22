using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;
using Milou.Deployer.Web.IisHost.Areas.ErrorHandling;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
{
    public static class DeploymentApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddForwardHeaders(
            this IApplicationBuilder app,
            EnvironmentConfiguration environmentConfiguration)
        {
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
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

        public static IApplicationBuilder UseSignalRHubs(this IApplicationBuilder app)
        {
            return app.UseSignalR(builder => builder.MapHub<DeploymentLoggingHub>(DeploymentLogConstants.HubRoute));
        }

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
    }
}
