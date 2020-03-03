using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.Agent.Host.Deployment
{
    [UsedImplicitly]
    public class DeploymentTaskAgentResultHandler : IRequestHandler<DeploymentTaskAgentResult>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeploymentTaskAgentResultHandler(IHttpClientFactory httpClientFactory) =>
            _httpClientFactory = httpClientFactory;

        public async Task<Unit> Handle(DeploymentTaskAgentResult request, CancellationToken cancellationToken)
        {
            Uri uri = new Uri("http://localhost:34343/deployment-task/result"); //TODO fix hard coded URL
            await _httpClientFactory.CreateClient().PostAsJsonAsync(uri, request, cancellationToken);

            return Unit.Value;
        }
    }
}