using System;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.WorkTasks
{
    public class DeploymentTaskCreatedNotification : INotification
    {
        public DeploymentTaskCreatedNotification([NotNull] DeploymentTask deploymentTask)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            DeploymentTask =
                new DeploymentTaskCreated(deploymentTask.DeploymentTaskId, deploymentTask.PackageId,
                    deploymentTask.DeploymentTargetId, deploymentTask.StartedBy);
        }

        public DeploymentTaskCreated DeploymentTask { get; }
    }
}