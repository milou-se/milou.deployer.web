using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [Urn(HostNameUrn)]
    [Optional]
    [UsedImplicitly]
    public class AllowedHostName
    {
        public const string HostNameUrn = "urn:milou:deployer:web:allowed-host";

        public AllowedHostName(string hostName)
        {
            HostName = hostName;
        }

        public string HostName { get; }
    }
}