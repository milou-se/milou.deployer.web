using Arbor.KVConfiguration.Core;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    public static class AppSettingsBuilderExtensions
    {
        public static AppSettingsBuilder Add(this AppSettingsBuilder builder, AppSettingsBuilder next)
        {
            return builder.Add(next.KeyValueConfiguration);
        }
    }
}
