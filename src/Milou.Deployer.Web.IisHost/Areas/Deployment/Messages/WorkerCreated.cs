using MediatR;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Messages
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