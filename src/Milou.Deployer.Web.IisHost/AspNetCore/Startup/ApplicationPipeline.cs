using System.IO;

using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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
            var environmentConfiguration = app!.ApplicationServices.GetRequiredService<EnvironmentConfiguration>();

            app.AddForwardHeaders(environmentConfiguration);

            app.AddRequestLogging(environmentConfiguration);

            app.AddExceptionHandling(environmentConfiguration);

            app.UseMiddleware<StartupTasksMiddleware>();

            app.UseMiddleware<RedirectMiddleware>();

            app.UseCustomStaticFiles(environmentConfiguration);

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<ConfigurationErrorMiddleware>();

            app.UseEndpoints(
                options =>
                {
                    options.MapControllers();
                    options.UseSignalRHubs();
                });
        }
    }
}
