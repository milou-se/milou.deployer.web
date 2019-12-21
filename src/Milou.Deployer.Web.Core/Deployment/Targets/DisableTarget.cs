using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class DisableTarget : IRequest
    {
        public DisableTarget(string targetId)
        {
            TargetId = targetId;
        }

        public string TargetId { get; }
    }
}