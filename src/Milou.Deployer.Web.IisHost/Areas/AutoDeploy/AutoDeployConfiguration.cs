using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.AutoDeploy
{
    [Urn(ConfigurationConstants.AutoDeployConfiguration)]
    [UsedImplicitly]
    public class AutoDeployConfiguration
    {
        public AutoDeployConfiguration(int startupDelayInSeconds, int metadataTimeoutInSeconds, bool enabled, int defaultTimeoutInSeconds, int afterDeployDelayInSeconds)
        {
            StartupDelayInSeconds = startupDelayInSeconds >= 0 ? metadataTimeoutInSeconds : 0;
            MetadataTimeoutInSeconds = metadataTimeoutInSeconds > 0 ? metadataTimeoutInSeconds : 5;
            Enabled = enabled;
            AfterDeployDelayInSeconds = afterDeployDelayInSeconds <= 0 ? 10 : afterDeployDelayInSeconds;
            DefaultTimeoutInSeconds = defaultTimeoutInSeconds <= 0 ? 30 : defaultTimeoutInSeconds;
        }

        public bool Enabled { get; }

        public int StartupDelayInSeconds { get; }

        public int MetadataTimeoutInSeconds { get; }

        public int DefaultTimeoutInSeconds { get; }

        public int AfterDeployDelayInSeconds { get; private set; }
    }
}