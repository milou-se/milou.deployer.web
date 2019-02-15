using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class PreReleaseBehavior
    {
        public static readonly PreReleaseBehavior Invalid = new PreReleaseBehavior(nameof(Invalid));

        public static readonly PreReleaseBehavior AllowWithForceFlag =
            new PreReleaseBehavior(nameof(AllowWithForceFlag));

        public static readonly PreReleaseBehavior Allow = new PreReleaseBehavior(nameof(Allow));

        public static readonly PreReleaseBehavior Deny = new PreReleaseBehavior(nameof(Deny));

        private PreReleaseBehavior(string name)
        {
            Name = name;
        }

        [PublicAPI]
        public string Name { get; }

        [PublicAPI]
        public static IReadOnlyCollection<PreReleaseBehavior> All { get; } = new[]
        {
            Invalid,
            AllowWithForceFlag,
            Allow,
            Deny
        };

        public static PreReleaseBehavior Parse(string value)
        {
            return All.SingleOrDefault(
                       behavior => behavior.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase)) ?? Invalid;
        }
    }
}