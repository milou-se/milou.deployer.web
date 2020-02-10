using Arbor.KVConfiguration.Core.Metadata;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Configuration
{
    public class DeployerAppConstants
    {

        [Metadata]
        public const string EmailConfiguration = "urn:milou:deployer:web:email:configuration";

        [PublicAPI]
        [Metadata(defaultValue: "false", valueType: "bool")]
        public const string EmailConfigurationEnabledDefault = EmailConfiguration + ":default:EmailEnabled";

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
        public const string SeedTimeoutInSeconds = "urn:milou:deployer:web:seed-timeout-in-seconds";

        [Metadata]
        public const string StartupTargetsTimeoutInSeconds =
            "urn:milou:deployer:web:startup:targets-timeout-in-seconds";

        [Metadata]
        public const string NuGetDownloadTimeoutInSeconds = "urn:milou:deployer:web:nuget:download-timeout-in-seconds";

        [Metadata]
        public const string NuGetExeDirectory = "urn:milou:deployer:web:nuget:nuget-directory";

        [Metadata]
        public const string AllowedIPs = "urn:milou:deployer:authorization:allowed-ips";

        [Metadata]
        public const string AllowedIpNetworks = "urn:milou:deployer:authorization:allowed-ip-networks";

        [Metadata]
        public const string NuGetExeVersion = "urn:milou:deployer:web:nuget:nuget-exe-version";
    }
}