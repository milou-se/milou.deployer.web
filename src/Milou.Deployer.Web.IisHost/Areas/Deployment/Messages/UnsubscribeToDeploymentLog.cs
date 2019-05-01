using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Messages
{
    public class UnsubscribeToDeploymentLog : IRequest
    {
        public UnsubscribeToDeploymentLog(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }
    }
}
