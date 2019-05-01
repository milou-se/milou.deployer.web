using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class StartWorker : IRequest
    {
        public StartWorker(DeploymentTargetWorker worker)
        {
            Worker = worker;
        }

        public DeploymentTargetWorker Worker { get; }
    }
}
