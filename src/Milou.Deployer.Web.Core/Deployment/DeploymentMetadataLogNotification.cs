using System;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentMetadataLogNotification : INotification
    {
        public DeploymentMetadataLogNotification(
            [NotNull] DeploymentTask deploymentTask,
            [NotNull] DeploymentTaskResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (string.IsNullOrWhiteSpace(result.Metadata))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(result));
            }

            DeploymentTask = deploymentTask ?? throw new ArgumentNullException(nameof(deploymentTask));
            Result = result;
        }

        public DeploymentTask DeploymentTask { get; }

        [NotNull]
        public DeploymentTaskResult Result { get; }

        public override string ToString()
        {
            return $"{nameof(DeploymentTask)}: {DeploymentTask}, {nameof(Result)}: {Result}";
        }
    }
}