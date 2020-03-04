using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Agent.Host.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace Milou.Deployer.Web.Agent.Host.Deployment
{
    public class DeploymentTaskPackageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly TokenConfiguration _tokenConfiguration;

        public DeploymentTaskPackageService(IHttpClientFactory httpClientFactory, ILogger logger, TokenConfiguration tokenConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _tokenConfiguration = tokenConfiguration;
        }

        public async Task<DeploymentTaskPackage?> GetDeploymentTaskPackageAsync(string deploymentTaskId,
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenConfiguration.Key);

            string json =
                await httpClient.GetStringAsync($"{_tokenConfiguration.ServerBaseUri}{AgentConstants.DeploymentTaskPackageRoute.Replace("{deploymentTaskId}",deploymentTaskId)}");

            //TODO fix http error handling

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            DeploymentTaskPackage deploymentTaskPackage;
            try
            {

                deploymentTaskPackage = JsonConvert.DeserializeObject<DeploymentTaskPackage>(json);
            }
            catch (Exception ex) when(!ex.IsFatal())
            {
                _logger.Error(ex, "Could not deserialize deployment task package from json {Json}", json);
                return null;
            }

            return deploymentTaskPackage;
        }
    }
}