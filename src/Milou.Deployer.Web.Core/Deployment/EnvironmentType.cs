using System;
using System.Collections.Generic;
using System.Linq;

namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class EnvironmentType
    {
        public static readonly EnvironmentType Invalid =
            new EnvironmentType(nameof(Invalid), PreReleaseBehavior.Invalid);

        public static readonly EnvironmentType QA =
            new EnvironmentType(nameof(QA), PreReleaseBehavior.AllowWithForceFlag);

        public static readonly EnvironmentType Production =
            new EnvironmentType(nameof(Production), PreReleaseBehavior.Deny);

        public static readonly EnvironmentType Integration =
            new EnvironmentType(nameof(Integration), PreReleaseBehavior.Allow);

        public static readonly EnvironmentType Development =
            new EnvironmentType(nameof(Development), PreReleaseBehavior.Allow);

        private EnvironmentType(string name, PreReleaseBehavior preReleaseBehavior)
        {
            Name = name;
            PreReleaseBehavior = preReleaseBehavior;
        }

        public string Name { get; }

        public PreReleaseBehavior PreReleaseBehavior { get; }

        public static IReadOnlyCollection<EnvironmentType> All => new[]
        {
            Invalid,
            QA,
            Production,
            Integration,
            Development
        };

        public static EnvironmentType Parse(string value)
        {
            return All.SingleOrDefault(
                       environmentType =>
                           environmentType.Name.Equals(value,
                               StringComparison.InvariantCultureIgnoreCase)) ?? Invalid;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}