using Arbor.App.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;

namespace Milou.Deployer.Web.IisHost.Areas.Agents
{
    public class AgentModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) =>
            builder.AddSingleton<IAgentService, RemoteAgentService>()
                .AddSingleton<AgentHub>();
    }
}