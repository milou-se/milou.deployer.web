using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
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
    }
}