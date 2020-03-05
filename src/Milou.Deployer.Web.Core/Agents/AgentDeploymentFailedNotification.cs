using MediatR;

namespace Milou.Deployer.Web.Core.Agents
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