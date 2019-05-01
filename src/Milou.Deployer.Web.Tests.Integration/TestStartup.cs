using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Milou.Deployer.Web.Tests.Integration.TestData;

namespace Milou.Deployer.Web.Tests.Integration
{
    [PublicAPI]
    public class TestStartup
    {
        public static TestConfiguration TestConfiguration { get; set; }

        [PublicAPI]
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(TestConfiguration.SiteAppRoot.FullName),
                RequestPath = new PathString("")
            });
        }
    }
}
