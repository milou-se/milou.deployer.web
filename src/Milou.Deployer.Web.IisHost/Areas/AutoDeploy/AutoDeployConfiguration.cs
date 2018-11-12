using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.AutoDeploy
{
    [Urn(ConfigurationConstants.AutoDeployConfiguration)]
    [UsedImplicitly]
    public class AutoDeployConfiguration : IConfigurationValues
    {
        public AutoDeployConfiguration(
            int startupDelayInSeconds,
            int metadataTimeoutInSeconds,
            bool enabled,
            int defaultTimeoutInSeconds,
            int afterDeployDelayInSeconds,
            int emptyTargetsDelayInSeconds)
        {
            StartupDelayInSeconds = startupDelayInSeconds >= 0 ? metadataTimeoutInSeconds : 0;
            MetadataTimeoutInSeconds = metadataTimeoutInSeconds > 0 ? metadataTimeoutInSeconds : 5;
            Enabled = enabled;
            AfterDeployDelayInSeconds = afterDeployDelayInSeconds <= 0 ? 10 : afterDeployDelayInSeconds;
            DefaultTimeoutInSeconds = defaultTimeoutInSeconds <= 0 ? 30 : defaultTimeoutInSeconds;
            EmptyTargetsDelayInSeconds = emptyTargetsDelayInSeconds <= 0 ? 60 : emptyTargetsDelayInSeconds;
        }

        public int EmptyTargetsDelayInSeconds { get; }

        public bool Enabled { get; }

        public int StartupDelayInSeconds { get; }

        public int MetadataTimeoutInSeconds { get; }

        public int DefaultTimeoutInSeconds { get; }

        public int AfterDeployDelayInSeconds { get; }

        public override string ToString()
        {
            return $"{nameof(EmptyTargetsDelayInSeconds)}: {EmptyTargetsDelayInSeconds}, {nameof(Enabled)}: {Enabled}, {nameof(StartupDelayInSeconds)}: {StartupDelayInSeconds}, {nameof(MetadataTimeoutInSeconds)}: {MetadataTimeoutInSeconds}, {nameof(DefaultTimeoutInSeconds)}: {DefaultTimeoutInSeconds}, {nameof(AfterDeployDelayInSeconds)}: {AfterDeployDelayInSeconds}";
        }
    }
}