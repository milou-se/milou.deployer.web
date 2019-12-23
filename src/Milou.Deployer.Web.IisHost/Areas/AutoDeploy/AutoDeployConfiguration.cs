using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.AutoDeploy
{
    [Urn(DeployerAppConstants.AutoDeployConfiguration)]
    [UsedImplicitly]
    public class AutoDeployConfiguration : IConfigurationValues
    {
        public AutoDeployConfiguration(
            int startupDelayInSeconds,
            int metadataTimeoutInSeconds,
            int defaultTimeoutInSeconds,
            int afterDeployDelayInSeconds,
            int emptyTargetsDelayInSeconds)
        {
            StartupDelayInSeconds = startupDelayInSeconds >= 0 ? metadataTimeoutInSeconds : 0;
            MetadataTimeoutInSeconds = metadataTimeoutInSeconds > 0 ? metadataTimeoutInSeconds : 5;
            AfterDeployDelayInSeconds = afterDeployDelayInSeconds <= 0 ? 10 : afterDeployDelayInSeconds;
            DefaultTimeoutInSeconds = defaultTimeoutInSeconds <= 0 ? 30 : defaultTimeoutInSeconds;
            EmptyTargetsDelayInSeconds = emptyTargetsDelayInSeconds <= 0 ? 5 : emptyTargetsDelayInSeconds;
        }

        public int EmptyTargetsDelayInSeconds { get; }

        public int StartupDelayInSeconds { get; }

        public int MetadataTimeoutInSeconds { get; }

        public int DefaultTimeoutInSeconds { get; }

        public int AfterDeployDelayInSeconds { get; }

        public override string ToString()
        {
            return
                $"{nameof(EmptyTargetsDelayInSeconds)}: {EmptyTargetsDelayInSeconds}, {nameof(StartupDelayInSeconds)}: {StartupDelayInSeconds}, {nameof(MetadataTimeoutInSeconds)}: {MetadataTimeoutInSeconds}, {nameof(DefaultTimeoutInSeconds)}: {DefaultTimeoutInSeconds}, {nameof(AfterDeployDelayInSeconds)}: {AfterDeployDelayInSeconds}";
        }
    }
}
