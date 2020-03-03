using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Deployment
{
    public interface IDeploymentTargetWorker
    {
        string TargetId { get; }

        bool IsRunning { get; }

        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}