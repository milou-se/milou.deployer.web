using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Milou.Deployer.Web.Agent
{
    public class CustomHttpClient : IHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _deploymentTaskId;
        private readonly string _deploymentTargetId;

        public CustomHttpClient(IHttpClientFactory httpClientFactory, string deploymentTaskId, string deploymentTargetId)
        {
            _httpClientFactory = httpClientFactory;
            _deploymentTaskId = deploymentTaskId;
            _deploymentTargetId = deploymentTargetId;
        }

        public void Dispose() {}

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            var httpClient = _httpClientFactory.CreateClient("AgentLogger");

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