using System.Collections.Immutable;

using Arbor.KVConfiguration.Core;

using JetBrains.Annotations;

using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class UpdateDeploymentTargetResult : ITargetResult, INotification
    {
        public UpdateDeploymentTargetResult(string targetName, string targetId, params ValidationError[] validationErrors)
        {
            TargetName = targetName;
            TargetId = targetId;
            ValidationErrors = validationErrors.ToImmutableArray();
        }

        public string TargetName { get; }

        public string TargetId { get; }

        [PublicAPI]
        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}