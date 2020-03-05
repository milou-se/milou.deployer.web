using MediatR;

namespace Milou.Deployer.Web.Core.Agents
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