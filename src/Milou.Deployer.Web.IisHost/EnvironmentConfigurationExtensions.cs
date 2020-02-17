using Arbor.App.Extensions.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Milou.Deployer.Web.IisHost
{
    public static class EnvironmentConfigurationExtensions
    {
        public static IHostEnvironment ToHostEnvironment(this EnvironmentConfiguration environmentConfiguration) =>
            new HostingEnvironment {EnvironmentName = environmentConfiguration.EnvironmentName};
    }
}