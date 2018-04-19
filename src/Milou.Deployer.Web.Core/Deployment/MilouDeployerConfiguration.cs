using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class MilouDeployerConfiguration
    {
        public MilouDeployerConfiguration(string milouDeployerExePath = "", string logLevel= "")
        {
            MilouDeployerExePath = milouDeployerExePath.WithDefault(
                StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.DeployerExePath]
                    .ThrowIfEmpty(ConfigurationConstants.DeployerExePath));

            LogLevel = logLevel.WithDefault(StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.Logging.LogLevel]);
        }

        public string LogLevel { get; set; }

        public string MilouDeployerExePath { get;  }
    }
}