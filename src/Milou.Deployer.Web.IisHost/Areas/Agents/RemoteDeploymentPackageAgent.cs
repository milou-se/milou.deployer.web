using System;
using System.Threading;
using System.Threading.Tasks;
using Arbor.Processing;
using Microsoft.AspNetCore.SignalR;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;

namespace Milou.Deployer.Web.Agent
{
    public class RemoteDeploymentPackageAgent : IDeploymentPackageAgent
    {
        private readonly AgentHub _agentHub;
        private readonly string _agentId;

        public RemoteDeploymentPackageAgent(AgentHub agentHub, string agentId)
        {
            _agentHub = agentHub;
            _agentId = agentId;
        }

        public async Task<ExitCode> RunAsync(string deploymentTaskId, CancellationToken cancellationToken = default)
        {
            //TODO use agent id
            await _agentHub.Clients.All.SendAsync("Deploy", deploymentTaskId);

            await Task.Delay(TimeSpan.FromSeconds(10)); //TODO

            return ExitCode.Success;
        }
    }
}