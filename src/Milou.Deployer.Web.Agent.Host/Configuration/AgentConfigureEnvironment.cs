using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.Configuration;

namespace Milou.Deployer.Web.Agent.Host.Configuration
{
    public class AgentConfigureEnvironment : IConfigureEnvironment
    {
        public void Configure(EnvironmentConfiguration environmentConfiguration) =>
            environmentConfiguration.HttpEnabled = false;
    }
}