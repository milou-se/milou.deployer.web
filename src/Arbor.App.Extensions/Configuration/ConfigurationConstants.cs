using Arbor.KVConfiguration.Core.Metadata;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Configuration
{
    public static class ConfigurationConstants
    {
        [Metadata]
        public const string JsonSourceEnabled = "urn:arbor:app:web:json-source:enabled";

        public const string ContentBasePath = "urn:arbor:app:web:content-base-path";

        [Metadata]
        public const string RestartTimeInSeconds = "urn:arbor:app:web:restart-time-in-seconds";

        [Metadata(defaultValue: "0")]
        public const string ShutdownTimeInSeconds = "urn:arbor:app:web:shutdown-time-in-seconds";

        [Metadata]
        public const string NuGetPackageSourceName = "urn:arbor:app:web:nuget:package-source-name";

        [Metadata]
        public const string NugetConfigFile = "urn:arbor:app:web:nuget:config";

        [Metadata]
        public const string JsonDeployerTargetSourceConfiguration = "urn:arbor:app:web:target-source:json";

        public const string SecretsKeyPrefix = "urn:arbor:app:web:secrets:";

        [PublicAPI]
        [Metadata]
        public const string SettingsPath = "urn:arbor:app:web:settings-path";


        [Metadata]
        public const string ApplicationBasePath = "urn:arbor:app:web:application-base-path";

        [Metadata]
        public const string JsonSettingsFile = "urn:arbor:app:web:settings:json-file";


        [Metadata]
        public const string LogLevel = "urn:arbor:app:web:log:level";
    }
}