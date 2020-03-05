using System.Net.Http;
using Milou.Deployer.Web.Agent.Host.Configuration;
using Serilog.Sinks.Http;

namespace Milou.Deployer.Web.Agent.Host.Logging
{
    public class LogHttpClientFactory
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AgentConfiguration _agentConfiguration;

        public LogHttpClientFactory(IHttpClientFactory clientFactory, AgentConfiguration agentConfiguration)
        {
            _clientFactory = clientFactory;
            _agentConfiguration = agentConfiguration;
        }

        public IHttpClient CreateClient(string deploymentTaskId, string deploymentTargetId) =>
            new CustomHttpClient(_clientFactory, deploymentTaskId, deploymentTargetId, _agentConfiguration);
    }
}