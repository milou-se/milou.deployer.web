using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using NuGet.Versioning;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public class AppVersion
    {
        private readonly DateTimeOffset _utcNow;

        public AppVersion(
            [NotNull] DeploymentTarget target,
            [NotNull] IKeyValueConfiguration manifestProperties,
            IReadOnlyCollection<PackageVersion> availablePackageVersions, DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
            Properties =
                manifestProperties ?? throw new ArgumentNullException(nameof(manifestProperties));
            AvailablePackageVersions = availablePackageVersions;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Status = GetStatus();
        }

        public AppVersion([NotNull] DeploymentTarget target, string message, IReadOnlyCollection<PackageVersion> availablePackgages)
        {
            Properties = new InMemoryKeyValueConfiguration(new NameValueCollection());
            Target = target;
            Message = message;
            AvailablePackageVersions = availablePackgages;
            Status = GetStatus();
        }

        private DeployStatus GetStatus()
        {
            if (SemanticVersion is null)
            {
                return DeployStatus.Unavailable;
            }

            if (AvailablePackageVersions.Count == 0)
            {
                return DeployStatus.NoPackagesAvailable;
            }

            return SemanticVersion == AvailablePackageVersions.Latest() ? DeployStatus.Latest : DeployStatus.UpdateAvailable;
        }

        public string Message { get; }

        [NotNull]
        public IKeyValueConfiguration Properties { get; }

        [NotNull]
        public DeploymentTarget Target { get; }

        [CanBeNull]
        public SemanticVersion SemanticVersion
        {
            get
            {
                if (!SemanticVersion.TryParse(Properties[ConfigurationConstants.SemanticVersionNormalized],
                    out SemanticVersion semver))
                {
                    return null;
                }

                return semver;
            }
        }

        [CanBeNull]
        public string PackageId => Properties[ConfigurationConstants.PackageId];

        [CanBeNull]
        public DateTime? DeployedAtUtc
        {
            get
            {
                if (!DateTime.TryParse(
                    Properties[ConfigurationConstants.DeploymentStartTime],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal,
                    out DateTime deployedAtUtc))
                {
                    return null;
                }

                return deployedAtUtc;
            }
        }

        public string PreReleaseClass
        {
            get
            {
                if (SemanticVersion == null)
                {
                    return string.Empty;
                }

                if (SemanticVersion.IsPrerelease)
                {
                    return "prerelease";
                }

                return "stable";
            }
        }

        public IReadOnlyCollection<PackageVersion> AvailablePackageVersions { get; }

        public DeployStatus Status { get;}
    }

    public sealed class DeployStatus
    {
        public static readonly DeployStatus Latest = new DeployStatus("latest", "Latest");
        public static readonly DeployStatus NoPackagesAvailable = new DeployStatus("no-packages", "No packages available");
        public static readonly DeployStatus UpdateAvailable = new DeployStatus("update-available", "Update available");
        public static readonly DeployStatus Unavailable = new DeployStatus("unavailable", "Unavailable");

        private DeployStatus(string key, string displayName)
        {
            Key = key;
            DisplayName = displayName;
        }

        public string Key { get; }

        public string DisplayName { get; }
    }
}