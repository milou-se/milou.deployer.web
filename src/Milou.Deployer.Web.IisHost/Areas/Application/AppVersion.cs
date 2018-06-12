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
        }

        public AppVersion([NotNull] DeploymentTarget target, string message, IReadOnlyCollection<PackageVersion> availablePackgages)
        {
            Properties = new InMemoryKeyValueConfiguration(new NameValueCollection());
            Target = target;
            Message = message;
            AvailablePackageVersions = availablePackgages;
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
        public DateTime? DateployedAtUtc
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

        public string DeployedRelative
        {
            get
            {
                if (!DateployedAtUtc.HasValue)
                {
                    return "N/A";
                }

                DateTime now = _utcNow.DateTime;

                DateTime then = DateployedAtUtc.Value;

                return now.Since(then);
            }
        }

        public string DeployedSince
        {
            get
            {
                if (!DateployedAtUtc.HasValue)
                {
                    return string.Empty;
                }

                DateTime now = _utcNow.DateTime;

                DateTime then = DateployedAtUtc.Value;

                TimeSpan diff = (now - then);

                if (diff.TotalSeconds < 0)
                {
                    return string.Empty;
                }

                return DeploymentInterval.Parse(diff).Name;
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
    }
}