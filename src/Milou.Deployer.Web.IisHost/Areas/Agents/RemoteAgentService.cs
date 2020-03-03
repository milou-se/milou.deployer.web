using System;
using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;

namespace Milou.Deployer.Web.IisHost.Areas.Agents
{
    public class RemoteAgentService : IAgentService
    {
        private readonly AgentHub _agentHub;

        public RemoteAgentService(AgentHub agentHub)
        {
            _agentHub = agentHub;
        }

        public async Task<IDeploymentPackageAgent> GetAgentForDeploymentTask(DeploymentTask deploymentTask,
            CancellationToken cancellationToken)
        {
            if (_agentHub.AgentIds.Length == 0)
            {
                throw new InvalidOperationException("No agent available");
            }

            if (_agentHub.AgentIds.Length == 1)
            {
                string agentId = _agentHub.AgentIds[0];

                return new RemoteDeploymentPackageAgent(_agentHub, agentId);
            }

            throw new NotSupportedException("Does not yet support multiple agents");
        }
    }
}