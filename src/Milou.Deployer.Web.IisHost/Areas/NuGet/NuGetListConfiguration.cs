using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
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
