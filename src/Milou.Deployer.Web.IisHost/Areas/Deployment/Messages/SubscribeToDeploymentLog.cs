using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Messages
{
    public class SubscribeToDeploymentLog : IRequest
    {
        public SubscribeToDeploymentLog(string connectionId, string deploymentTargetId)
        {
            ConnectionId = connectionId;
            DeploymentTargetId = deploymentTargetId;
        }

        public string ConnectionId { get; }
        public string DeploymentTargetId { get; }
    }
}
