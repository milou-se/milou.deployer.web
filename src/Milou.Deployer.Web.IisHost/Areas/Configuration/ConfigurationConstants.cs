using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    public static class ConfigurationConstants
    {
        [Metadata()]
        public static string JsonSourceEnabled = "urn:milou:deployer:web:json-source:enabled";

        [Metadata()]
        public const string HostConfiguration = "urn:milou:deployer:web:host";

        [Metadata()]
        public const string JsonSettingsFile = "urn:milou:deployer:web:settings:json-settings-file-path";

        [Metadata()]
        public const string RestartTimeInSeconds = "urn:milou:deployer:web:restart-time-in-seconds";

        [Metadata()]
        public const string SettingsPath = "urn:milou:deployer:web:settings-path";
    }
}
