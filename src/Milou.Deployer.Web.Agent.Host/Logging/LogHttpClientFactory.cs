using System.Net.Http;
using Milou.Deployer.Web.Agent.Host.Configuration;
using Serilog.Sinks.Http;

namespace Milou.Deployer.Web.Agent.Host.Logging
{
    public class LogHttpClientFactory
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly TokenConfiguration _tokenConfiguration;

        public LogHttpClientFactory(IHttpClientFactory clientFactory, TokenConfiguration tokenConfiguration)
        {
            _clientFactory = clientFactory;
            _tokenConfiguration = tokenConfiguration;
        }

        public IHttpClient CreateClient(string deploymentTaskId, string deploymentTargetId) =>
            new CustomHttpClient(_clientFactory, deploymentTaskId, deploymentTargetId, _tokenConfiguration);
    }
}