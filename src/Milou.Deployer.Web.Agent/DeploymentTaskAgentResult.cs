using MediatR;

namespace Milou.Deployer.Web.Agent
{
    public class DeploymentTaskAgentResult : IRequest<Unit>
    {
        public DeploymentTaskAgentResult(string deploymentTaskId, string deploymentTargetId, bool succeeded)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
            Succeeded = succeeded;
        }

        public string DeploymentTaskId { get; }

        public string DeploymentTargetId { get; }

        public bool Succeeded { get; }
    }
}