using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Core.Targets
{
    [Urn(ConfigurationConstants.JsonDeployerTargetSourceConfiguration)]
    [UsedImplicitly]
    public class JsonDeploymentTargetSourceConfiguration
    {
        public JsonDeploymentTargetSourceConfiguration(string sourceFile)
        {
            SourceFile = sourceFile;
        }

        public string SourceFile { get; }

        public bool Enabled { get; }
    }
}