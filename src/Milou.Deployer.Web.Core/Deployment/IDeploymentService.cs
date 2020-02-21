using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Serilog;

namespace Milou.Deployer.Web.Agent
{
    public interface IDeploymentService
    {
        Task<DeploymentTaskResult> ExecuteDeploymentAsync(
            [NotNull] DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken);
    }
}