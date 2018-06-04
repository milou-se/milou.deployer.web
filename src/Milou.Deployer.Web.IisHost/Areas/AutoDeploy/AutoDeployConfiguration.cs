using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.AutoDeploy
{
    [Urn(ConfigurationConstants.AutoDeployConfiguration)]
    [UsedImplicitly]
    public class AutoDeployConfiguration
    {
        public AutoDeployConfiguration(int startupDelayInSeconds, int metadataTimeoutInSeconds, bool enabled)
        {
            StartupDelayInSeconds = startupDelayInSeconds >= 0 ? metadataTimeoutInSeconds : 0;
            MetadataTimeoutInSeconds = metadataTimeoutInSeconds > 0 ? metadataTimeoutInSeconds : 5;
            Enabled = enabled;
        }

        public bool Enabled { get; }

        public int StartupDelayInSeconds { get; }

        public int MetadataTimeoutInSeconds { get; }
    }
}