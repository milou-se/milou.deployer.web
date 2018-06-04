using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class MilouDeployerConfiguration
    {
        public MilouDeployerConfiguration(
            IKeyValueConfiguration keyValueConfiguration,
            string milouDeployerExePath = "",
            string logLevel = "")
        {
            MilouDeployerExePath = milouDeployerExePath.WithDefault(
                keyValueConfiguration[ConfigurationConstants.DeployerExePath]
                    .ThrowIfEmpty(ConfigurationConstants.DeployerExePath));

            LogLevel = logLevel.WithDefault(keyValueConfiguration[ConfigurationConstants.Logging.LogLevel]);
        }

        public string LogLevel { get; }

        public string MilouDeployerExePath { get; }
    }
}