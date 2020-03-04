using System.Globalization;
using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Configuration;
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

        public bool IsValid => !Enabled || !string.IsNullOrWhiteSpace(ConnectionString);

        public override string ToString() =>
            $"{nameof(ConnectionString)}: [{ConnectionString.MakeKeyValuePairAnonymous(ApplicationStringExtensions.DefaultAnonymousKeyWords.ToArray())}], {nameof(Enabled)}: {Enabled.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()}";
    }
}