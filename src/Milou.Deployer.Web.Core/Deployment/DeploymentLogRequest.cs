using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentLogRequest : IRequest<DeploymentLogResponse>
    {
        public string DeploymentTaskId { get; }

        public DeploymentLogRequest(string deploymentTaskId)
        {
            DeploymentTaskId = deploymentTaskId;
        }

    }

    public class DeploymentLogResponse
    {
        public DeploymentLogResponse(string log)
        {
            Log = log;
        }

        public string Log { get; }
    }
}