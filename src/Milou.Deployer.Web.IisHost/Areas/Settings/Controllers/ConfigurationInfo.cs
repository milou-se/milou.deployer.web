using System.Collections.Generic;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ConfigurationInfo
    {
        public string ConfigurationSourceChain { get; }

        public IReadOnlyCollection<ConfigurationKeyInfo> Keys { get; }

        public ConfigurationInfo(string configurationSourceChain, IReadOnlyCollection<ConfigurationKeyInfo> keys)
        {
            ConfigurationSourceChain = configurationSourceChain;
            Keys = keys;
        }
    }
}