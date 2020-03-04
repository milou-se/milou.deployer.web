using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Agent.Host.Configuration
{
    [UsedImplicitly]
    [Urn(Urn)]
    [Optional]
    public class TokenConfiguration : IConfigurationValues
    {
        public const string Urn = "urn:milou:deployer:web:agent:bearer-token";

        public TokenConfiguration(string key, string serverBaseUri)
        {
            Key = key;
            ServerBaseUri = serverBaseUri;
        }

        public string Key { get; }

        public string ServerBaseUri { get; }
    }
}