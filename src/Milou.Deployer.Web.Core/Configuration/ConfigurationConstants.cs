using Arbor.KVConfiguration.Core.Metadata;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Configuration
{
    public static class ConfigurationConstants
    {
        [Metadata]
        public const string JsonSourceEnabled = "urn:milou:deployer:web:json-source:enabled";

        public const string ContentBasePath = "urn:milou:deployer:web:content-base-path";

        [Metadata]
        public const string RestartTimeInSeconds = "urn:milou:deployer:web:restart-time-in-seconds";

        [Metadata(defaultValue: "0")]
        public const string ShutdownTimeInSeconds = "urn:milou:deployer:web:shutdown-time-in-seconds";

        [Metadata]
        public const string NuGetPackageSourceName = "urn:milou:deployer:web:nuget:package-source-name";

        [Metadata]
        public const string NugetConfigFile = "urn:milou:deployer:web:nuget:config";

        [Metadata]
        public const string JsonDeployerTargetSourceConfiguration = "urn:milou:deployer:web:target-source:json";

        public const string SecretsKeyPrefix = "urn:milou:deployer:web:secrets:";

        [PublicAPI]
        [Metadata]
        public const string SettingsPath = "urn:milou:deployer:web:settings-path";

        [Metadata]
        public const string EmailConfiguration = "urn:milou:deployer:web:email:configuration";

        [PublicAPI]
        [Metadata(defaultValue: "false", valueType: "bool")]
        public const string EmailConfigurationEnabledDefault = EmailConfiguration + ":default:EmailEnabled";

        [Metadata]
        public const string ApplicationBasePath = "urn:milou:deployer:web:application-base-path";

        [Metadata]
        public const string AllowPreReleaseEnabled = "MilouDeployer_AllowPreRelease_Enabled";

        [Metadata]
        public const string DeployerTarget = "urn:milou-deployer:target";

        [Metadata]
        public const string DeploymentStartTime = "urn:deployment:start-time-utc";

        [Metadata]
        public const string SemanticVersionNormalized = "urn:versioning:semver2:normalized";

        public const string PackageId = "urn:nuget:package-id";

        public const string AutoDeployConfiguration = "urn:milou:deployer:web:auto-deploy:configuration";

        [PublicAPI]
        [Metadata(defaultValue: "false")]
        public const string AutoDeployConfigurationDefaultEnabled = AutoDeployConfiguration + ":default:enabled";

        [Metadata]
        public static string AllowedIPs = "urn:milou:deployer:authorization:allowed-ips";

        [Metadata]
        public static string JsonSettingsFile = "urn:milou:deployer:settings:json-file";

        [Metadata]
        public const string SeedTimeoutInSeconds = "urn:milou:deployer:web:seed-timeout-in-seconds";

        [Metadata]
        public const string StartupTargetsTimeoutInSeconds ="urn:milou:deployer:web:startup:targets-timeout-in-seconds";

        public static class Logging
        {
            [PublicAPI]
            [Metadata]
            public const string SerilogLogFilePath = "urn:milou-deployer:logging:serilog:log-file:path";

            [PublicAPI]
            [Metadata]
            public const string LogLevel = "urn:milou-deployer:web:log:level";

            [PublicAPI]
            [Metadata]
            public const string SerilogSeqUrl = "urn:milou-deployer:logging:serilog:seq:url";

            [PublicAPI]
            [Metadata]
            public const string SerilogMinLevel = "urn:milou-deployer:logging:serilog:min-level";

            [PublicAPI]
            [Metadata]
            public const string SerilogFileLogEnabled = "urn:milou-deployer:logging:serilog:log-file:enabled";

            [PublicAPI]
            [Metadata]
            public const string SerilogConsoleLogEnabled = "urn:milou-deployer:logging:serilog:console:enabled";
        }
    }
}