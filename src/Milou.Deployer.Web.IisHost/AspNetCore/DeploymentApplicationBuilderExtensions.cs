using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public static class DeploymentApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddForwardHeaders(this IApplicationBuilder app)
        {
            return app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
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

            return app.UseExceptionHandler("/error");
        }
    }
}