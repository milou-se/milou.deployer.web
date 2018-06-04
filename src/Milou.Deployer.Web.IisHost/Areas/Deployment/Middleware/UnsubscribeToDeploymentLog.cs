using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    public class UnsubscribeToDeploymentLog : IRequest
    {
        public string ConnectionId { get; }

        public UnsubscribeToDeploymentLog(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}