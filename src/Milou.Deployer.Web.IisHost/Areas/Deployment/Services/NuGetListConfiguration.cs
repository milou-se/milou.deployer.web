using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [Urn(NuGetListConstants.Configuration)]
    [UsedImplicitly]
    public class NuGetListConfiguration
    {
        public int ListTimeOutInSeconds { get; }

        public NuGetListConfiguration(int listTimeOutInSeconds)
        {
            ListTimeOutInSeconds = listTimeOutInSeconds <= 0 ? 30 : listTimeOutInSeconds;
        }
    }
}