using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
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
