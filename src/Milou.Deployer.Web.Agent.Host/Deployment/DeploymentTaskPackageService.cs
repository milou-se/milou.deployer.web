using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Milou.Deployer.Web.Agent.Host.Configuration;
using Serilog;

namespace Milou.Deployer.Web.Agent.Host.Deployment
{
    public class DeploymentTaskPackageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public DeploymentTaskPackageService(IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<DeploymentTaskPackage?> GetDeploymentTaskPackageAsync(string deploymentTaskId,
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpConfigurationModule.AgentClient);

            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{AgentConstants.DeploymentTaskPackageRoute.Replace("{deploymentTaskId}", deploymentTaskId)}");

            return await httpClient.TrySendAndReadResponseJson<DeploymentTaskPackage>(request, _logger,
                cancellationToken);
        }
    }
}