using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.ErrorHandling;
using Milou.Deployer.Web.IisHost.Areas.Logging;
using Milou.Deployer.Web.IisHost.Areas.Routing;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
{
    [PublicAPI]
    public class ApplicationPipeline
    {
        public void Configure(IApplicationBuilder app)
        {
            var environmentConfiguration = app.ApplicationServices.GetService<EnvironmentConfiguration>();

            app.AddForwardHeaders(environmentConfiguration);

            app.AddRequestLogging(environmentConfiguration);

            app.AddExceptionHandling(environmentConfiguration);

            app.UseMiddleware<StartupTasksMiddleware>();

            app.UseMiddleware<RedirectMiddleware>();

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMiddleware<ConfigurationErrorMiddleware>();

            app.UseSignalRHubs();

            app.UseMvc();
        }
    }
}
