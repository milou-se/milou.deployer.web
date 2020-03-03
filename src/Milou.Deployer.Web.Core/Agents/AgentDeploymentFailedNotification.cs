using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling
{
    public class AgentDeploymentFailedNotification : INotification
    {
        public string DeploymentTaskId { get; }
        public string DeploymentTargetId { get; }

        public AgentDeploymentFailedNotification(string deploymentTaskId, string deploymentTargetId)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
        }
    }
}