using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class TargetDisabled : INotification
    {
        public TargetDisabled(string targetId)
        {
            TargetId = targetId;
        }

        public string TargetId { get; }
    }
}