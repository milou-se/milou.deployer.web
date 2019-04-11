using Arbor.KVConfiguration.Core.Metadata;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Core.Logging
{
    [UsedImplicitly]
    [Urn(HttpLoggingConfigurationUrn)]
    public class HttpLoggingConfiguration : IConfigurationValues
    {
        [Metadata(defaultValue: "true")]
        public const string HttpLoggingEnabled = "urn:milou:deployer:web:http-logging:default:enabled";

        [Metadata]
        public const string HttpLoggingConfigurationUrn = "urn:milou:deployer:web:http-logging";

        public HttpLoggingConfiguration(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }
}
