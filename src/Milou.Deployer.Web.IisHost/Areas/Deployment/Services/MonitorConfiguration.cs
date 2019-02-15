using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [Urn(MonitorConstants.MonitorConfiguration)]
    [UsedImplicitly]
    public class MonitorConfiguration : IConfigurationValues
    {
        public int DefaultTimeoutInSeconds { get; }

        public MonitorConfiguration(int defaultTimeoutInSeconds)
        {
            DefaultTimeoutInSeconds = defaultTimeoutInSeconds > 0 ? defaultTimeoutInSeconds : 10;
        }

        public override string ToString()
        {
            return $"{nameof(DefaultTimeoutInSeconds)}: {DefaultTimeoutInSeconds}";
        }
    }
}