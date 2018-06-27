using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [Urn(MonitorConstants.MonitorConfiguration)]
    [UsedImplicitly]
    public class MonitorConfiguration
    {
        public int DefaultTimeoutInSeconds { get; }

        public MonitorConfiguration(int defaultTimeoutInSeconds)
        {
            DefaultTimeoutInSeconds = defaultTimeoutInSeconds > 0 ? defaultTimeoutInSeconds : 10;
        }
    }
}