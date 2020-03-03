using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class WorkerCreated : INotification
    {
        public IDeploymentTargetWorker Worker { get; }

        public WorkerCreated(IDeploymentTargetWorker worker) => Worker = worker;
    }
}