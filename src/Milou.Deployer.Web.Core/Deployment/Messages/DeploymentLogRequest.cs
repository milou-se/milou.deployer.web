using MediatR;
using Serilog.Events;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class DeploymentLogRequest : IRequest<DeploymentLogResponse>
    {
        public DeploymentLogRequest(string deploymentTaskId, LogEventLevel level)
        {
            DeploymentTaskId = deploymentTaskId;
            Level = level;
        }

        public string DeploymentTaskId { get; }

        public LogEventLevel Level { get; }
    }
}
