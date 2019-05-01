using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
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
