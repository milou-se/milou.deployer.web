using Arbor.App.Extensions;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class EnvironmentType
    {
        [PublicAPI]
        public static readonly EnvironmentType Unknown =
            new EnvironmentType("", nameof(Unknown), PreReleaseBehavior.Invalid);

        public EnvironmentType(string id, string name, PreReleaseBehavior preReleaseBehavior)
        {
            Id = id;
            Name = name;
            PreReleaseBehavior = preReleaseBehavior;
        }

        public string Id { get; }

        public string Name { get; }

        public PreReleaseBehavior PreReleaseBehavior { get; }

        public override string ToString()
        {
            if (Equals(Unknown))
            {
                return Constants.NotAvailable;
            }

            return Name;
        }
    }
}