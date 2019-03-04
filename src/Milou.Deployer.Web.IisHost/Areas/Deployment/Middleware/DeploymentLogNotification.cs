using System;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    public class DeploymentLogNotification : INotification
    {
        public DeploymentLogNotification([NotNull] string deploymentTargetId, [NotNull] string message)
        {
            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            }

            DeploymentTargetId = deploymentTargetId;
            Message = message;
        }

        public string DeploymentTargetId { get; }

        public string Message { get; }
    }
}
