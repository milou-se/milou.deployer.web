using System;

using Milou.Deployer.Web.Core.Integration.Nexus;

namespace Milou.Deployer.Web.Core.Settings
{
    public class ApplicationSettings
    {
        public TimeSpan CacheTime { get; set; }

        public NexusConfig NexusConfig { get; set; } = new NexusConfig();

        public AutoDeploySettings AutoDeploy { get; set; }

        public TimeSpan ApplicationSettingsCacheTimeout { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan DefaultMetadataRequestTimeout { get; set; }

        public TimeSpan MetadataCacheTimeout { get; set; }
    }
}