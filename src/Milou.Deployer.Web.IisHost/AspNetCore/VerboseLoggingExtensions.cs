using Microsoft.AspNetCore.Builder;
using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public static class VerboseLoggingExtensions
    {
        public static IApplicationBuilder AddRequestLogging(
            this IApplicationBuilder app,
            EnvironmentConfiguration environmentConfiguration)
        {
            if (environmentConfiguration.UseVerboseLogging)
            {
                return app.UseMiddleware<VerboseLoggingMiddleware>();
            }

            return app;
        }
    }
}
