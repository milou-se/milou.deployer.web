using System.Threading;
using System.Threading.Tasks;
using Arbor.Processing;
using Serilog;

namespace Milou.Deployer.Web.Agent
{
    /// <summary>
    /// Runs deploy for all given data defined in deployment task package
    /// </summary>
    public interface IDeploymentPackageHandler
    {
        Task<ExitCode> RunAsync(
            DeploymentTaskPackage deploymentTaskPackage,
            ILogger jobLogger,
            CancellationToken cancellationToken);
    }
}