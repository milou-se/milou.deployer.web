using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class EnableTarget : IRequest
    {
        public EnableTarget(string targetId)
        {
            TargetId = targetId;
        }

        public string TargetId { get; }
    }
}
