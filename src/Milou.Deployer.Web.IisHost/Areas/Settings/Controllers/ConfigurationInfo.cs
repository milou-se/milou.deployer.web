﻿using System.Collections.Generic;
using Arbor.App.Extensions.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ConfigurationInfo
    {
        public ConfigurationInfo(string configurationSourceChain, IReadOnlyCollection<ConfigurationKeyInfo> keys)
        {
            ConfigurationSourceChain = configurationSourceChain;
            Keys = keys;
        }

        public string ConfigurationSourceChain { get; }

        public IReadOnlyCollection<ConfigurationKeyInfo> Keys { get; }
    }
}
