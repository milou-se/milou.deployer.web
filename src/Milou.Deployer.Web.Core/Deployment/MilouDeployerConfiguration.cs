using Arbor.App.Extensions;
using Arbor.App.Extensions.Configuration;
using Arbor.App.Extensions.ExtensionMethods;
using Arbor.KVConfiguration.Core;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class MilouDeployerConfiguration
    {
        public MilouDeployerConfiguration(
            IKeyValueConfiguration keyValueConfiguration,
            string logLevel = "")
        {
            LogLevel = logLevel.WithDefault(keyValueConfiguration[ConfigurationConstants.LogLevel]);
        }

        public string LogLevel { get; }
    }
}
