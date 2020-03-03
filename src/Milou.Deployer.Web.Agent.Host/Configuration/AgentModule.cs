using Arbor.App.Extensions.Configuration;
using Arbor.App.Extensions.DependencyInjection;
using Arbor.App.Extensions.Time;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Agent.Host.Deployment;
using Milou.Deployer.Web.Agent.Host.Logging;

namespace Milou.Deployer.Web.Agent.Host.Configuration
{
    [UsedImplicitly]
    public class AgentModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            builder.AddSingleton(
                new TimeoutHelper(new TimeoutConfiguration {CancellationEnabled = false}));

            builder.AddSingleton<DeploymentTaskPackageService>();
            builder.AddSingleton<LogHttpClientFactory>();
            builder.AddSingleton<IDeploymentPackageAgent, DeploymentPackageAgent>();
            builder.AddSingleton<IDeploymentPackageHandler, DeploymentPackageHandler>();
            builder.AddSingleton<IConfigureEnvironment, AgentConfigureEnvironment>();

            return builder;
        }
    }
}