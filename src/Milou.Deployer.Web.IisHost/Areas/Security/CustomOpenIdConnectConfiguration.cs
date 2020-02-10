using Arbor.App.Extensions.Configuration;

using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [Urn(UrnKey)]
    [UsedImplicitly]
    [Optional]
    public class CustomOpenIdConnectConfiguration : IConfigurationValues
    {
        [PublicAPI]
        public const string UrnKey = "urn:milou:deployer:web:open-id-connect";

        public CustomOpenIdConnectConfiguration(
            string clientId,
            string clientSecret,
            string authority,
            string metadataAddress,
            string issuer,
            bool enabled)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Authority = authority;
            MetadataAddress = metadataAddress;
            Issuer = issuer;
            Enabled = enabled;
            IsValid = !enabled || (!string.IsNullOrWhiteSpace(clientId)
                                   && !string.IsNullOrWhiteSpace(clientSecret)
                                   && (!string.IsNullOrWhiteSpace(authority) ||
                                       !string.IsNullOrWhiteSpace(metadataAddress)));
        }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string Authority { get; }

        public string MetadataAddress { get; }

        public string Issuer { get; }

        public bool Enabled { get; }

        public bool IsValid { get; }
    }
}
