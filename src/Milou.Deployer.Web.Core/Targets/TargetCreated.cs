using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class TargetCreated : INotification
    {
        public TargetCreated(string targetId)
        {
            TargetId = targetId;
        }

        public string TargetId { get; }
    }
}