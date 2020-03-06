using System.Net.Http;
using Serilog.Sinks.Http;

namespace Milou.Deployer.Web.Agent.Host.Logging
{
    public class LogHttpClientFactory
    {
        private readonly IHttpClientFactory _clientFactory;

        public LogHttpClientFactory(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

        public IHttpClient CreateClient(string deploymentTaskId, string deploymentTargetId) =>
            new CustomHttpClient(_clientFactory, deploymentTaskId, deploymentTargetId);
    }
}