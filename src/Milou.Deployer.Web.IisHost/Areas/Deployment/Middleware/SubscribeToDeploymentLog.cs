using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
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
