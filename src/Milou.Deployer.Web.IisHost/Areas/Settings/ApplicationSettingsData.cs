using System;

using Milou.Deployer.Web.Core.Integration.Nexus;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class ApplicationSettingsData
    {
        public TimeSpan? CacheTime { get; set; }

        public string Id { get; set; }

        public NexusConfigData? NexusConfig { get; set; }
    }
}