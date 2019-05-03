using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class EnableTarget : IRequest
    {
        public string TargetId { get; }

        public EnableTarget(string targetId)
        {
            TargetId = targetId;
        }
    }
}
