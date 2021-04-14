using System.Globalization;
using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Configuration;
using Arbor.App.Extensions.ExtensionMethods;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Marten
{
    [Urn(MartenConstants.MartenConfiguration)]
    [UsedImplicitly]
    [Optional]
    public class MartenConfiguration : IConfigurationValues
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
            return
                $"{nameof(ConnectionString)}: [{ConnectionString.MakeKeyValuePairAnonymous(ArborStringExtensions.DefaultAnonymousKeyWords.ToArray())}], {nameof(Enabled)}: {Enabled.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()}";
        }

        public bool IsValid => !Enabled || !string.IsNullOrWhiteSpace(ConnectionString);
    }
}
