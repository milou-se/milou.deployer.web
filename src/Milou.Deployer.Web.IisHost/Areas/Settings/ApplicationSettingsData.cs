using System;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class ApplicationSettingsData
    {
        public TimeSpan? CacheTime { get; set; }

        public string Id { get; set; }
    }
}