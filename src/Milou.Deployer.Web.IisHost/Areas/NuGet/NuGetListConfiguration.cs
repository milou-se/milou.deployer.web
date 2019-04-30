using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [Urn(NuGetListConstants.Configuration)]
    [UsedImplicitly]
    public class NuGetListConfiguration : IConfigurationValues
    {
        public NuGetListConfiguration(int listTimeOutInSeconds)
        {
            ListTimeOutInSeconds = listTimeOutInSeconds <= 0 ? 7 : listTimeOutInSeconds;
        }

        public int ListTimeOutInSeconds { get; }
    }
}
