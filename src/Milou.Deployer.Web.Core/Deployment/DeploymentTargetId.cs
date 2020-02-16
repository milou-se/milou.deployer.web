using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentTargetId
    {
        public DeploymentTargetId([NotNull] string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            TargetId = targetId;
        }

        public string TargetId { get; }

        public override string ToString() => TargetId;
    }
}