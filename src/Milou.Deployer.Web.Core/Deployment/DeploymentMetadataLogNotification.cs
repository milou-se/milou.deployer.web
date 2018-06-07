using System;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentMetadataLogNotification : INotification
    {
        public DeploymentMetadataLogNotification(
            [NotNull] DeploymentTask deploymentTask,
            [NotNull] string metadataContent)
        {
            if (string.IsNullOrWhiteSpace(metadataContent))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(metadataContent));
            }

            DeploymentTask = deploymentTask ?? throw new ArgumentNullException(nameof(deploymentTask));
            MetadataContent = metadataContent;
        }

        public DeploymentTask DeploymentTask { get; }

        public string MetadataContent { get; }
    }
}