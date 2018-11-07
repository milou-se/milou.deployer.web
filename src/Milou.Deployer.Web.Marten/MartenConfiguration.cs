using System.Linq;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.Marten
{
    [Urn(MartenConstants.MartenConfiguration)]
    [UsedImplicitly]
    public class MartenConfiguration : IValidationObject, IConfigurationValues
    {
        public MartenConfiguration(string connectionString, bool enabled = false)
        {
            ConnectionString = connectionString;
            Enabled = enabled;
        }

        public string ConnectionString { get; }

        public bool Enabled { get; }

        public bool IsValid => !Enabled || !string.IsNullOrWhiteSpace(ConnectionString);

        public override string ToString()
        {
            return
                $"{nameof(ConnectionString)}: [{ConnectionString.MakeKeyValuePairAnonymous(StringExtensions.DefaultAnonymousKeyWords.ToArray())}], {nameof(Enabled)}: {Enabled.ToString().ToLowerInvariant()}";
        }
    }
}