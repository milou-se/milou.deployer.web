using System.Collections.Immutable;

using Arbor.KVConfiguration.Core;

using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class UpdateDeploymentTargetResult : ITargetResult
    {
        public UpdateDeploymentTargetResult(string targetName, params ValidationError[] validationErrors)
        {
            TargetName = targetName;
            ValidationErrors = validationErrors.ToImmutableArray();
        }

        public string TargetName { get; }

        [PublicAPI]
        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}