using Arbor.AspNetCore.Host.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Security;

namespace Milou.Deployer.Web.IisHost
{
    public static class DeployerOpenIdExtensions
    {
        public static void RegisterOpenId(this ServiceProviderHolder serviceProviderHolder)
        {
            //var openIdConnectConfiguration = serviceProviderHolder.ServiceProvider.GetService<CustomOpenIdConnectConfiguration>();

            //var httpLoggingConfiguration = serviceProviderHolder.ServiceProvider.GetService<HttpLoggingConfiguration>();

            //var milouAuthenticationConfiguration = serviceProviderHolder.ServiceProvider.GetService<MilouAuthenticationConfiguration>();

            //services.AddDeploymentAuthentication(openIdConnectConfiguration, milouAuthenticationConfiguration, logger, environmentConfiguration)
            //    .AddDeploymentAuthorization(environmentConfiguration)
            //    .AddDeploymentHttpClients(httpLoggingConfiguration)
            //    .AddDeploymentSignalR()
            //    .AddServerFeatures()
            //    .AddDeploymentMvc();
        }
    }
}