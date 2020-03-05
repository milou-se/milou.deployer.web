using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Agent.Host.Configuration
{
    [UsedImplicitly]
    public class HttpConfigurationModule : IModule
    {
        public const string AgentClient = "Agent";

        public const string AgentLoggerClient = "AgentLogger";

        public IServiceCollection Register(IServiceCollection builder)
        {
            builder.AddHttpClient(AgentClient, AddedBearerToken);
            builder.AddHttpClient(AgentLoggerClient, AddedBearerToken);

            return builder;
        }

        private void AddedBearerToken(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var agentConfiguration = serviceProvider.GetRequiredService<AgentConfiguration>();

            httpClient.BaseAddress = new Uri(agentConfiguration.ServerBaseUri);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", agentConfiguration.AccessToken);
        }
    }
}