using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentHistoryRequest : IRequest<DeploymentHistoryResponse>
    {
        public string DeploymentTargetId { get; }

        public DeploymentHistoryRequest(string deploymentTargetId)
        {
            DeploymentTargetId = deploymentTargetId;
        }
    }
}