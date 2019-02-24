using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [Urn(UrnKey)]
    public class CustomOpenIdConnectConfiguration : IValidationObject, IConfigurationValues
    {
        public const string UrnKey = "urn:milou:deployer:web:open-id-connect";

        public CustomOpenIdConnectConfiguration(
            string clientId,
            string clientSecret,
            string authority,
            string metadataAddress,
            string issuer,
            bool enabled,
            string redirectUri,
            string authenticatedRedirectUri)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Authority = authority;
            MetadataAddress = metadataAddress;
            Issuer = issuer;
            Enabled = enabled;
            RedirectUri = redirectUri;
            AuthenticatedRedirectUri = authenticatedRedirectUri;
            IsValid = !string.IsNullOrWhiteSpace(clientId)
                      && !string.IsNullOrWhiteSpace(clientSecret)
                      && (!string.IsNullOrWhiteSpace(authority) ||
                          !string.IsNullOrWhiteSpace(metadataAddress));
        }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string Authority { get; }

        public string MetadataAddress { get; }

        public string Issuer { get; }

        public bool Enabled { get; }

        public string RedirectUri { get; }

        public bool IsValid { get; }

        public string AuthenticatedRedirectUri { get; }
    }
}