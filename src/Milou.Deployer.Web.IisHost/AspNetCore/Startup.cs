using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [PublicAPI]
    public class Startup
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
