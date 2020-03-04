using System;
using Milou.Deployer.Web.Marten.AutoDeploy;

namespace Milou.Deployer.Web.Marten.Settings
{
    [MartenData]
    public class ApplicationSettingsData
    {
        public TimeSpan? CacheTime { get; set; }

        public string Id { get; set; }

        public NexusConfigData? NexusConfig { get; set; }

        public AutoDeployData AutoDeploy { get; set; }

        public TimeSpan DefaultMetadataTimeout { get; set; }

        public TimeSpan ApplicationSettingsCacheTimeout { get; set; }

        public TimeSpan MetadataCacheTimeout { get; set; }
    }
}