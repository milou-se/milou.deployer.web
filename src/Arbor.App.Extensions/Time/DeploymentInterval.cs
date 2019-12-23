using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Time
{
    [PublicAPI]
    public struct DeploymentInterval : IEquatable<DeploymentInterval>
    {
        public bool Equals(DeploymentInterval other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal) && FromExclusive == other.FromExclusive && ToInclusive == other.ToInclusive;
        }

        public override bool Equals(object obj)
        {
            return obj is DeploymentInterval other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name.GetHashCode(StringComparison.InvariantCulture);
                hashCode = (hashCode * 397) ^ FromExclusive;
                hashCode = (hashCode * 397) ^ ToInclusive;
                return hashCode;
            }
        }

        public static bool operator ==(DeploymentInterval left, DeploymentInterval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DeploymentInterval left, DeploymentInterval right)
        {
            return !left.Equals(right);
        }

        public static readonly DeploymentInterval Invalid = new DeploymentInterval(nameof(Invalid), int.MinValue, -1);

        public static readonly DeploymentInterval ThisWeek = new DeploymentInterval(nameof(ThisWeek), -1, 7);

        public static readonly DeploymentInterval ThisMonth = new DeploymentInterval(nameof(ThisMonth), 7, 30);

        public static readonly DeploymentInterval ThisQuarter = new DeploymentInterval(nameof(ThisQuarter), 30, 90);

        public static readonly DeploymentInterval ThisYear = new DeploymentInterval(nameof(ThisYear), 90, 365);

        public static readonly DeploymentInterval MoreThanAYear = new DeploymentInterval(
            nameof(MoreThanAYear),
            365,
            int.MaxValue);

        public DeploymentInterval(string name, int fromExclusive, int toInclusive)
        {
            Name = name;
            FromExclusive = fromExclusive;
            ToInclusive = toInclusive;
        }

        public string Name { get; }

        public int FromExclusive { get; }

        public int ToInclusive { get; }

        public static IReadOnlyCollection<DeploymentInterval> All => new[]
        {
            Invalid,
            ThisWeek,
            ThisMonth,
            ThisQuarter,
            ThisYear,
            MoreThanAYear
        };

        public static DeploymentInterval Parse(TimeSpan timeSpan)
        {
            return All.Single(
                interval => timeSpan.TotalDays > interval.FromExclusive && timeSpan.TotalDays <= interval.ToInclusive);
        }

        public override string ToString()
        {
            if (Equals(Invalid))
            {
                return Constants.NotAvailable;
            }

            return Name;
        }
    }
}
