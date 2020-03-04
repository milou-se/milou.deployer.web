using System.Net.Http;
using System.Threading.Tasks;
using Milou.Deployer.Web.Agent.Host.Configuration;
using Serilog.Sinks.Http;

namespace Milou.Deployer.Web.Agent.Host.Logging
{
    public class CustomHttpClient : IHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _deploymentTaskId;
        private readonly string _deploymentTargetId;
        private readonly TokenConfiguration _tokenConfiguration;

        public CustomHttpClient(IHttpClientFactory httpClientFactory, string deploymentTaskId, string deploymentTargetId, TokenConfiguration tokenConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _deploymentTaskId = deploymentTaskId;
            _deploymentTargetId = deploymentTargetId;
            _tokenConfiguration = tokenConfiguration;
        }

        public void Dispose() {}

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            var httpClient = _httpClientFactory.CreateClient("AgentLogger");

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenConfiguration.Key);


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };

            request.Headers.Add("x-deployment-task-id", _deploymentTaskId);
            request.Headers.Add("x-deployment-target-id", _deploymentTargetId);
            var httpResponseMessage = await httpClient.SendAsync(request);

            return httpResponseMessage;
        }
    }
}