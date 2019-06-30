using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Core.Deployment;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Deployment
{
    [Optional]
    [Urn(ConfigurationConstants.DeployerTarget)]
    public class DeploymentTarget
    {
        public static readonly DeploymentTarget None =
            new DeploymentTarget(Constants.NotAvailable, Constants.NotAvailable, Constants.NotAvailable);

        public DeploymentTarget(
            [NotNull] string id,
            [NotNull] string name,
            string packageId,
            string publishSettingsXml = null,
            bool allowExplicitPreRelease = false,
            string nuGetConfigFile = null,
            string nuGetPackageSource = null,
            Uri url = null,
            string environmentConfiguration = null,
            string organization = null,
            string project = null,
            bool autoDeployment = false,
            string environmentType = null,
            bool autoDeployEnabled = false,
            StringValues emailNotificationAddresses = default,
            Dictionary<string, string[]> parameters = null,
            string publishSettingFile = null,
            string targetDirectory = null,
            string parameterFile = null,
            bool isReadOnly = false,
            string iisSiteName = default,
            string webConfigTransform = default,
            string excludedFilePatterns = default,
            bool enabled = false,
            TimeSpan? packageListTimeout = default,
            string publishType = default,
            string ftpPath = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            _ = PublishType.TryParseOrDefault(publishType, out PublishType type);
            PublishType = type;

            _ = FtpPath.TryParse(ftpPath, FileSystemType.Directory, out FtpPath path);
            FtpPath = path;

            Url = url;
            EnvironmentConfiguration = environmentConfiguration;
            AutoDeployment = autoDeployment;
            AutoDeployEnabled = autoDeployEnabled;
            PublishSettingFile = publishSettingFile;
            TargetDirectory = targetDirectory;
            ParameterFile = parameterFile;
            IsReadOnly = isReadOnly;
            IisSiteName = iisSiteName;
            WebConfigTransform = webConfigTransform;
            ExcludedFilePatterns = excludedFilePatterns;
            Enabled = enabled;
            PackageListTimeout = packageListTimeout;
            Organization = organization ?? string.Empty;
            ProjectInvariantName = project ?? string.Empty;
            Name = name;
            Id = id;
            AllowExplicitExplicitPreRelease = allowExplicitPreRelease;
            NuGetConfigFile = nuGetConfigFile;
            NuGetPackageSource = nuGetPackageSource;
            PackageId = packageId.WithDefault(Constants.NotAvailable);
            PublishSettingsXml = publishSettingsXml;
            EnvironmentType = EnvironmentType.Parse(environmentType);
            EmailNotificationAddresses = emailNotificationAddresses.SafeToReadOnlyCollection();
            Parameters = parameters?.ToImmutableDictionary() ?? ImmutableDictionary<string, string[]>.Empty;
        }

        public IReadOnlyCollection<string> EmailNotificationAddresses { get; }

        public Uri Url { get; }

        public string EnvironmentConfiguration { get; }

        public bool AutoDeployment { get; }

        public bool AutoDeployEnabled { get; }

        public string Organization { get; }

        public string ProjectInvariantName { get; }

        public string PackageId { get; }

        public bool? AllowExplicitExplicitPreRelease { get; }

        public bool AllowPreRelease
            =>
                AllowExplicitExplicitPreRelease.HasValue && AllowExplicitExplicitPreRelease.Value
                || EnvironmentType.PreReleaseBehavior == PreReleaseBehavior.Allow;

        public EnvironmentType EnvironmentType { get; }

        public string Id { get; }

        public string Name { get; }

        public string TargetDirectory { get; }

        public string PublishSettingFile { get; }

        public string PublishSettingsXml { get; }

        public string ParameterFile { get; }

        public bool IsReadOnly { get; }

        public string IisSiteName { get; }

        public string WebConfigTransform { get; }

        public string ExcludedFilePatterns { get; }

        public bool Enabled { get; }

        public ImmutableDictionary<string, string[]> Parameters { get; }

        public string NuGetConfigFile { get; }

        public string NuGetPackageSource { get; }

        public TimeSpan? PackageListTimeout { get; }

        [JsonProperty(nameof(PublishType))]
        public string PublishTypeValue => PublishType.Name;

        [JsonIgnore]
        public PublishType PublishType { get; }

        [JsonProperty(nameof(FtpPath))]
        public string FtpPathValue => FtpPath?.Path;

        [JsonIgnore]
        public FtpPath FtpPath { get; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Id))
            {
                return base.ToString();
            }

            return $"{Name} ({Id})";
        }
    }
}
