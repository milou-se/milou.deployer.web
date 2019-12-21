using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class EnvironmentType : IEquatable<EnvironmentType>
    {
        [PublicAPI]
        public static readonly EnvironmentType Invalid =
            new EnvironmentType(nameof(Invalid), PreReleaseBehavior.Invalid);

        [PublicAPI]
        public static readonly EnvironmentType QualityAssurance =
            new EnvironmentType(nameof(QualityAssurance), PreReleaseBehavior.AllowWithForceFlag);

        [PublicAPI]
        public static readonly EnvironmentType Production =
            new EnvironmentType(nameof(Production), PreReleaseBehavior.Deny);

        [PublicAPI]
        public static readonly EnvironmentType Integration =
            new EnvironmentType(nameof(Integration), PreReleaseBehavior.Allow);

        [PublicAPI]
        public static readonly EnvironmentType Development =
            new EnvironmentType(nameof(Development), PreReleaseBehavior.Allow);

        [PublicAPI]
        public static readonly EnvironmentType Test =
            new EnvironmentType(nameof(Test), PreReleaseBehavior.Allow);

        [PublicAPI]
        public static readonly EnvironmentType Stage =
            new EnvironmentType(nameof(Stage), PreReleaseBehavior.AllowWithForceFlag);

        private EnvironmentType(string name, PreReleaseBehavior preReleaseBehavior)
        {
            Name = name;
            PreReleaseBehavior = preReleaseBehavior;
        }

        public string Name { get; }

        public PreReleaseBehavior PreReleaseBehavior { get; }

        [PublicAPI]
        public static IReadOnlyCollection<EnvironmentType> All => new[]
        {
            Invalid,
            QualityAssurance,
            Production,
            Integration,
            Development,
            Test,
            Stage
        };

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

            return obj is EnvironmentType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0;
        }

        public static bool operator ==(EnvironmentType left, EnvironmentType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnvironmentType left, EnvironmentType right)
        {
            return !Equals(left, right);
        }

        public static EnvironmentType Parse(string value)
        {
            return All.SingleOrDefault(
                       environmentType =>
                           environmentType.Name.Equals(value,
                               StringComparison.InvariantCultureIgnoreCase)) ?? Invalid;
        }

        public override string ToString()
        {
            if (Equals(Invalid))
            {
                return Constants.NotAvailable;
            }

            return Name;
        }

        public bool Equals(EnvironmentType other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
