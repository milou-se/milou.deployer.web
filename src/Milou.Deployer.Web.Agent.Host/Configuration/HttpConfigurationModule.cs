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
            builder.AddHttpClient(AgentClient, ConfigureClient).ConfigurePrimaryHttpMessageHandler(Configure);
            builder.AddHttpClient(AgentLoggerClient, ConfigureClient).ConfigurePrimaryHttpMessageHandler(Configure);

            return builder;
        }

        private HttpMessageHandler Configure(IServiceProvider provider)
        {
            var agentConfiguration = provider.GetRequiredService<AgentConfiguration>();
            var httpMessageHandler = new HttpClientHandler();

            if (!agentConfiguration.CheckCertificateEnabled)
            {
                httpMessageHandler.ServerCertificateCustomValidationCallback = delegate
                {
                    return true;
                };
            }

            return httpMessageHandler;
        }

        private void ConfigureClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var agentConfiguration = serviceProvider.GetRequiredService<AgentConfiguration>();

            httpClient.BaseAddress = new Uri(agentConfiguration.ServerBaseUri);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", agentConfiguration.AccessToken);
        }
    }
}