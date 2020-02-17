using Arbor.App.Extensions.Application;
using Arbor.AspNetCore.Host;
using Arbor.AspNetCore.Host.Hosting;
using Arbor.KVConfiguration.Core;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Security;
using Milou.Deployer.Web.IisHost.AspNetCore.Startup;
using Serilog;

namespace Milou.Deployer.Web.IisHost
{
    public class DeployerOpenIdModule : IServiceProviderModule
    {
        public void Register(ServiceProviderHolder serviceProviderHolder)
        {
            var services = serviceProviderHolder.ServiceCollection;

            var openIdConnectConfiguration = serviceProviderHolder.ServiceProvider.GetService<CustomOpenIdConnectConfiguration>();

            var httpLoggingConfiguration = serviceProviderHolder.ServiceProvider.GetService<HttpLoggingConfiguration>();

            var milouAuthenticationConfiguration = serviceProviderHolder.ServiceProvider.GetService<MilouAuthenticationConfiguration>();

            var logger = serviceProviderHolder.ServiceProvider.GetRequiredService<ILogger>();
            var environmentConfiguration = serviceProviderHolder.ServiceProvider.GetRequiredService<EnvironmentConfiguration>();
            var configuration = serviceProviderHolder.ServiceProvider.GetRequiredService<IKeyValueConfiguration>();

            services.AddDeploymentAuthentication(openIdConnectConfiguration, milouAuthenticationConfiguration, logger, environmentConfiguration)
                .AddDeploymentAuthorization(environmentConfiguration)
                .AddDeploymentHttpClients(httpLoggingConfiguration)
                .AddDeploymentSignalR()
                .AddServerFeatures()
                .AddDeploymentMvc(environmentConfiguration, configuration);
        }
    }
}