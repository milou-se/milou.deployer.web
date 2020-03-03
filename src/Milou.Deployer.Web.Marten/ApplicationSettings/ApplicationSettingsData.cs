using System;

namespace Milou.Deployer.Web.Marten
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