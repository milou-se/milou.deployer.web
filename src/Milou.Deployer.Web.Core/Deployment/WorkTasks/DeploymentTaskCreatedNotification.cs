using MediatR;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;

namespace Milou.Deployer.Web.Marten
{
    public class DeploymentTaskCreatedNotification : INotification
    {
        public DeploymentTaskCreatedNotification(DeploymentTask deploymentTask)
        {
            DeploymentTask = new DeploymentTaskCreated(deploymentTask.DeploymentTaskId, deploymentTask.PackageId, deploymentTask.DeploymentTargetId, deploymentTask.StartedBy);
        }

        public DeploymentTaskCreated DeploymentTask { get; }
    }
}