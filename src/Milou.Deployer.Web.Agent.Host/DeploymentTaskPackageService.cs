using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Core.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace Milou.Deployer.Web.Agent
{
    public class DeploymentTaskPackageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public DeploymentTaskPackageService(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<DeploymentTaskPackage> GetDeploymentTaskPackageAsync(string deploymentTaskId,
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            string json =
                await httpClient.GetStringAsync($"http://localhost:34343/deployment-task-package/{deploymentTaskId}");

            //TODO fix http error handling
            //TODO fix hard coded url to deployment task package

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