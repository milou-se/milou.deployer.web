using System;
using System.Collections.Generic;
using System.Linq;

namespace Milou.Deployer.Web.Core
{
    public struct DeploymentInterval
    {
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