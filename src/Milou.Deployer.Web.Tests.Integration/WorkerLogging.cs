using System.Threading.Tasks;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Tests.Integration
{
    public static class WorkerLogging
    {
        public static Task LogAsync(Serilog.ILogger logger, string message, WorkTaskStatus status, string targetId)
        {
            logger.Information("{Message}", message);

            return Task.CompletedTask;
        }

        public static void Log(string message, WorkTaskStatus status, string deploymentTargetId)
        {

        }
    }
}