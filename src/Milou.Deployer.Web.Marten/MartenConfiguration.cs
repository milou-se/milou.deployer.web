using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Marten
{
    [Urn(MartenConstants.MartenConfiguration)]
    [UsedImplicitly]
    public class MartenConfiguration
    {
        public MartenConfiguration(string connectionString, bool enabled = false)
        {
            ConnectionString = connectionString;
            Enabled = enabled;
        }

        public string ConnectionString { get; }

        public bool Enabled { get; }

        public override string ToString()
        {
            return $"{nameof(ConnectionString)}: [{ConnectionString.MakeKeyValuePairAnonymous("user id", "password")}], {nameof(Enabled)}: {Enabled.ToString().ToLowerInvariant()}";
        }
    }
}