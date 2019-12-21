using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class TargetEnabled : INotification
    {
        public TargetEnabled(string targetId)
        {
            TargetId = targetId;
        }

        public string TargetId { get; }
    }
}
