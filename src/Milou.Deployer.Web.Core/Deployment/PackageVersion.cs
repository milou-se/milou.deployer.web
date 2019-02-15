using System;
using JetBrains.Annotations;
using NuGet.Versioning;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class PackageVersion : IEquatable<PackageVersion>
    {
        public PackageVersion(string packageId, SemanticVersion version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("Argument is null or whitespace", nameof(packageId));
            }

            PackageId = packageId;
            Version = version;
            Key = $"{PackageId}_{Version.ToNormalizedString()}";
        }

        public string PackageId { get; }

        public SemanticVersion Version { get; }

        [PublicAPI]
        public string Key { get; }

        public static bool operator ==(PackageVersion left, PackageVersion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackageVersion left, PackageVersion right)
        {
            return !Equals(left, right);
        }

        public bool Equals(PackageVersion other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((PackageVersion)obj);
        }

        public override int GetHashCode()
        {
            return Key?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Key;
        }
    }
}