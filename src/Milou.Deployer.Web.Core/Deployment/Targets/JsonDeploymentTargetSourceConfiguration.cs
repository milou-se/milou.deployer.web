using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    [Optional]
    [Urn(ConfigurationConstants.JsonDeployerTargetSourceConfiguration)]
    [UsedImplicitly]
    public class JsonDeploymentTargetSourceConfiguration
    {
        public JsonDeploymentTargetSourceConfiguration(string sourceFile, bool enabled = false)
        {
            SourceFile = sourceFile;
            Enabled = enabled;
        }

        public string SourceFile { get; }

        [PublicAPI]
        public bool Enabled { get; }
    }
}
