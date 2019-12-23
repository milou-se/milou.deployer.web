using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Packages;
using NuGet.Versioning;

namespace Milou.Deployer.Web.Core.Application.Metadata
{
    [PublicAPI]
    public class AppVersion
    {
        public AppVersion(
            [NotNull] DeploymentTarget target,
            [NotNull] IKeyValueConfiguration manifestProperties,
            IReadOnlyCollection<PackageVersion> availablePackageVersions)
        {
            Properties =
                manifestProperties ?? throw new ArgumentNullException(nameof(manifestProperties));
            AvailablePackageVersions = availablePackageVersions;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Status = GetStatus();
        }

        public AppVersion(
            [NotNull] DeploymentTarget target,
            string message,
            IReadOnlyCollection<PackageVersion> availablePackages)
        {
            Properties = new InMemoryKeyValueConfiguration(new NameValueCollection());
            Target = target;
            Message = message;
            AvailablePackageVersions = availablePackages;
            Status = GetStatus();
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
                if (!SemanticVersion.TryParse(Properties[DeployerAppConstants.SemanticVersionNormalized],
                    out var semver))
                {
                    return null;
                }

                return semver;
            }
        }

        [CanBeNull]
        public string PackageId => Properties[DeployerAppConstants.PackageId];

        [CanBeNull]
        public DateTime? DeployedAtUtc
        {
            get
            {
                if (!DateTime.TryParse(
                    Properties[DeployerAppConstants.DeploymentStartTime],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var deployedAtUtc))
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
                    // ReSharper disable once StringLiteralTypo
                    return "prerelease";
                }

                return "stable";
            }
        }

        public IReadOnlyCollection<PackageVersion> AvailablePackageVersions { get; }

        public DeployStatus Status { get; }

        public SemanticVersion LatestNewerAvailable { get; private set; }

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

            var latestAvailable = AvailablePackageVersions.Latest();

            if (SemanticVersion == latestAvailable)
            {
                return DeployStatus.Latest;
            }

            if (latestAvailable > SemanticVersion)
            {
                LatestNewerAvailable = latestAvailable;
                return DeployStatus.UpdateAvailable;
            }

            return DeployStatus.NoLaterAvailable;
        }
    }
}
