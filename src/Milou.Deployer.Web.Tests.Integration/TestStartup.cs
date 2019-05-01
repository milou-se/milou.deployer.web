using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Milou.Deployer.Web.Tests.Integration.TestData;

namespace Milou.Deployer.Web.Tests.Integration
{
    [PublicAPI]
    public class TestStartup
    {
        [PublicAPI]
        public void Configure(IApplicationBuilder app)
        {
            var testConfiguration = app.ApplicationServices.GetRequiredService<TestConfiguration>();

            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(testConfiguration.SiteAppRoot.FullName),
                RequestPath = new PathString("")
            };
            app.UseStaticFiles(staticFileOptions);
        }
    }
}
