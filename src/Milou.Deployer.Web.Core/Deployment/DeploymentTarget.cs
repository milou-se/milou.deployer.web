using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment
{
    [Urn(ConfigurationConstants.DeployerTarget)]
    public class DeploymentTarget
    {
        public DeploymentTarget(
            [NotNull] string id,
            [NotNull] string name,
            string tool,
            bool allowExplicitPreRelease,
            StringValues allowedPackageNames,
            string uri = null,
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
            bool isReadOnly = false)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri))
            {
                Uri = parsedUri;
            }

            EnvironmentConfiguration = environmentConfiguration;
            AutoDeployment = autoDeployment;
            AutoDeployEnabled = autoDeployEnabled;
            PublishSettingFile = publishSettingFile;
            TargetDirectory = targetDirectory;
            ParameterFile = parameterFile;
            IsReadOnly = isReadOnly;
            Organization = organization ?? string.Empty;
            ProjectInvariantName = project ?? string.Empty;
            Name = name;
            Id = id;
            Tool = tool ?? string.Empty;
            AllowExplicitExplicitPreRelease = allowExplicitPreRelease;
            AllowedPackageNames = allowedPackageNames.SafeToReadOnlyCollection();
            EnvironmentType = EnvironmentType.Parse(environmentType);
            EmailNotificationAddresses = emailNotificationAddresses.SafeToReadOnlyCollection();
            Parameters = parameters?.ToImmutableDictionary() ?? ImmutableDictionary<string, string[]>.Empty;
        }

        public IReadOnlyCollection<string> EmailNotificationAddresses { get; }

        public Uri Uri { get; }

        public string EnvironmentConfiguration { get; }

        public bool AutoDeployment { get; }

        public bool AutoDeployEnabled { get; }

        public string Organization { get; }

        public string ProjectInvariantName { get; }

        public IReadOnlyCollection<string> AllowedPackageNames { get; }

        public bool? AllowExplicitExplicitPreRelease { get; }

        public bool AllowPrerelease
            =>
                (AllowExplicitExplicitPreRelease.HasValue && AllowExplicitExplicitPreRelease.Value)
                || EnvironmentType.PreReleaseBehavior == PreReleaseBehavior.Allow;

        public EnvironmentType EnvironmentType { get; }

        public string Tool { get; }

        public string Id { get; }

        public string Name { get; }

        public string TargetDirectory { get; }

        public string PublishSettingFile { get; }

        public string ParameterFile { get; }

        public bool IsReadOnly { get; }

        public ImmutableDictionary<string, string[]> Parameters { get; }

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