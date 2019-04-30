using System;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [Urn(UrnKey)]
    [UsedImplicitly]

    [Optional]
    public class CustomOpenIdConnectConfiguration : IValidationObject, IConfigurationValues
    {
        [PublicAPI]
        public const string UrnKey = "urn:milou:deployer:web:open-id-connect";

        public CustomOpenIdConnectConfiguration(
            string clientId,
            string clientSecret,
            string authority,
            string metadataAddress,
            string issuer,
            bool enabled,
            string redirectUri)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Authority = authority;
            MetadataAddress = metadataAddress;
            Issuer = issuer;
            Enabled = enabled;
            if (Uri.TryCreate(redirectUri, UriKind.Absolute, out Uri uri))
            {
                RedirectUri = uri;
            }

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

        public Uri RedirectUri { get; }

        public bool IsValid { get; }
    }
}
