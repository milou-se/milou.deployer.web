using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [Urn(UrnKey)]
    [UsedImplicitly]
    [Optional]
    public class MilouAuthenticationConfiguration : IConfigurationValues
    {
        [PublicAPI] public const string UrnKey = "urn:milou:deployer:web:milou-authentication";

        public MilouAuthenticationConfiguration(bool enabled, bool bearerTokenEnabled, string? bearerTokenIssuerKey)
        {
            Enabled = enabled;
            BearerTokenEnabled = bearerTokenEnabled;
            BearerTokenIssuerKey = bearerTokenIssuerKey;
        }

        public bool Enabled { get; }

        public bool BearerTokenEnabled { get; }

        public string? BearerTokenIssuerKey { get; }
    }
}