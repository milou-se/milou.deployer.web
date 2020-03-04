using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Agent.Host.Configuration;

namespace Milou.Deployer.Web.Agent.Host.Deployment
{
    [UsedImplicitly]
    public class DeploymentTaskAgentResultHandler : IRequestHandler<DeploymentTaskAgentResult>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenConfiguration _tokenConfiguration;

        public DeploymentTaskAgentResultHandler(IHttpClientFactory httpClientFactory,
            TokenConfiguration tokenConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenConfiguration = tokenConfiguration;
        }

        public async Task<Unit> Handle(DeploymentTaskAgentResult request, CancellationToken cancellationToken)
        {
            Uri uri = new Uri(_tokenConfiguration.ServerBaseUri + AgentConstants.DeploymentTaskResult);
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenConfiguration.Key);

            await httpClient.PostAsJsonAsync(uri, request, cancellationToken);

            return Unit.Value;
        }
    }
}