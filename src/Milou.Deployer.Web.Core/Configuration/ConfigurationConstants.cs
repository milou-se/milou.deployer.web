using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.Core.Configuration
{
    public static class ConfigurationConstants
    {
        [Metadata]
        public const string JsonSourceEnabled = "urn:milou:deployer:web:json-source:enabled";

        public const string ContentBasePath = "urn:milou:deployer:web:content-base-path";

        [Metadata]
        public const string HostConfiguration = "urn:milou:deployer:web:host";

        [Metadata]
        public const string RestartTimeInSeconds = "urn:milou:deployer:web:restart-time-in-seconds";

        [Metadata(defaultValue: "0")]
        public const string ShutdownTimeInSeconds = "urn:milou:deployer:web:shutdown-time-in-seconds";

        [Metadata]
        public const string NuGetPackageSourceName = "tools:nuget:package-source-name";

        [Metadata]
        public const string NugetConfigFile = "tools:nuget:config-file";

        [Metadata]
        public const string NuGetExePath = "tools:nuget:exe-path";

        public const string DeployerTargetPrefix = "tools:milou-deployer:target";

        [Metadata]
        public const string JsonDeployerTargetSourceConfiguration = "urn:milou:deployer:web:target-source:json";

        public const string SecretsKeyPrefix = "urn:milou:deployer:web:secrets:";

        [Metadata]
        public const string SettingsPath = "urn:milou:deployer:web:settings-path";

        [Metadata]
        public const string EmailConfiguration = "urn:milou:deployer:web:email:configuration";

        [Metadata(defaultValue: "false", valueType: "bool")]
        public const string EmailConfigurationEnabledDefault = EmailConfiguration + ":default:EmailEnabled";

        [Metadata]
        public const string ApplicationBasePath = "urn:milou:deployer:web:application-base-path";

        [Metadata]
        public const string DeployerManifestEnabled = "tools:milou-deployer:use-manifest:enabled";

        [Metadata]
        public const string DeployerExePath = "urn:milou-deployer:web:deployer:exe-path";

        [Metadata]
        public const string AllowPreReleaseEnabled = "MilouDeployer_AllowPreRelease_Enabled";

        [Metadata]
        public const string DeployerTarget = "urn:milou-deployer:target";

        [Metadata]
        public const string DeploymentStartTime = "urn:deployment:start-time-utc";

        [Metadata]
        public const string SemanticVersionNormalized = "urn:versioning:semver2:normalized";

        public const string PackageId = "urn:nuget:package-id";

        [Metadata]
        public const string UsernameKey = "urn:authentication:basicauthentication:username";

        [Metadata]
        public const string PasswordKey = "urn:authentication:basicauthentication:password";

        public const string AutoDeployConfiguration = "urn:milou:deployer:auto-deploy:configuration";

        [Metadata(defaultValue: "false")]
        public const string AutoDeployConfigurationDefaultEnabled = "urn:milou:deployer:auto-deploy:configuration:default:enabled";

        [Metadata]
        public static string WhiteListedIPs = "urn:milou:deployer:authorization:white-listed-ips";

        [Metadata]
        public static string JsonSettingsFile = "urn:milou:deployer:settings:json-file";

        public static class Logging
        {
            [Metadata]
            public const string SerilogLogFilePath = "urn:milou-deployer:logging:serilog:log-file:path";

            [Metadata]
            public const string LogLevel = "tools:milou-deployer:log:level";

            [Metadata]
            public const string SerilogSeqUrl = "urn:milou-deployer:logging:serilog:seq:url";

            [Metadata]
            public const string SerilogMinLevel = "urn:milou-deployer:logging:serilog:min-level";

            [Metadata]
            public const string SerilogFileLogEnabled = "urn:milou-deployer:logging:serilog:log-file:enabled";

            [Metadata]
            public const string SerilogConsoleLogEnabled = "urn:milou-deployer:logging:serilog:console:enabled";
        }
    }
}