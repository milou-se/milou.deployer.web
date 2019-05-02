using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class RemoveTarget : IRequest
    {
        public string TargetId { get; }

        public RemoveTarget(string targetId)
        {
            TargetId = targetId;
        }
    }
}
