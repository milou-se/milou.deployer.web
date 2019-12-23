using Arbor.KVConfiguration.Core;

namespace Arbor.AspNetCore.Host.Configuration
{
    public static class AppSettingsBuilderExtensions
    {
        public static AppSettingsBuilder Add(this AppSettingsBuilder builder, AppSettingsBuilder next)
        {
            return builder.Add(next.KeyValueConfiguration);
        }
    }
}
