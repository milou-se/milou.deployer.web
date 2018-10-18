using Arbor.KVConfiguration.Core.Metadata;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Logging
{
    [UsedImplicitly]
    [Urn(HttpLoggingConfigurationUrn)]
    public class HttpLoggingConfiguration
    {
        public bool Enabled { get; }

        public HttpLoggingConfiguration(bool enabled)
        {
            Enabled = enabled;
        }

        [Metadata(defaultValue:"true")]
        public const string HttpLoggingEnabled = "urn:milou:deployer:web:http-logging:default:enabled";

        [Metadata]
        public const string HttpLoggingConfigurationUrn = "urn:milou:deployer:web:http-logging";
    }
}