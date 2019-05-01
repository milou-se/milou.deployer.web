using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class DeploymentLogRequest : IRequest<DeploymentLogResponse>
    {
        public DeploymentLogRequest(string deploymentTaskId)
        {
            DeploymentTaskId = deploymentTaskId;
        }

        public string DeploymentTaskId { get; }
    }
}
