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
        public bool Enabled { get; }

        [PublicAPI]
        public const string UrnKey = "urn:milou:deployer:web:milou-authentication";

        public MilouAuthenticationConfiguration(bool enabled)
        {
            Enabled = enabled;
        }
    }
}