using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling
{
    public class AgentDeploymentDoneNotification : INotification
    {
        public string DeploymentTaskId { get; }
        public string DeploymentTargetId { get; }

        public AgentDeploymentDoneNotification(string deploymentTaskId, string deploymentTargetId)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
        }
    }
}