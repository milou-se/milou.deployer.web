using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class WorkerCreated : INotification
    {
        public DeploymentTargetWorker Worker { get; }

        public WorkerCreated(DeploymentTargetWorker worker)
        {
            Worker = worker;
        }
    }
}