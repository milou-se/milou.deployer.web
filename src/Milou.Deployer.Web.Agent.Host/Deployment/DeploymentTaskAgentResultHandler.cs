﻿using System;
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
        private readonly AgentConfiguration _agentConfiguration;

        public DeploymentTaskAgentResultHandler(IHttpClientFactory httpClientFactory,
            AgentConfiguration agentConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _agentConfiguration = agentConfiguration;
        }

        public async Task<Unit> Handle(DeploymentTaskAgentResult request, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpConfigurationModule.AgentClient);

            await httpClient.PostAsJsonAsync(AgentConstants.DeploymentTaskResult, request, cancellationToken);

            return Unit.Value;
        }
    }
}