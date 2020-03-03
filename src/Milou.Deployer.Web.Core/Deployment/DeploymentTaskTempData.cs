using System.Collections.Generic;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Serilog;

namespace Milou.Deployer.Web.Core.Deployment
{
    internal class DeploymentTaskTempData
    {
        public DeploymentTaskTempData(ILogger tempLogger, string deploymentTaskId, List<LogItem> logBuilder)
        {
            TempLogger = tempLogger;
            DeploymentTaskId = deploymentTaskId;
            LogBuilder = logBuilder;
        }

        public ILogger TempLogger { get; }
        public string DeploymentTaskId { get; }
        public List<LogItem> LogBuilder { get; }
    }
}