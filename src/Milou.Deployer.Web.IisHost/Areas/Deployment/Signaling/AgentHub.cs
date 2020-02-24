using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Security;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling
{
    [Authorize(Policy = AuthorizationPolicies.Agent)]
    [UsedImplicitly]
    public class AgentHub : Hub
    {
        private readonly List<string> _agentIds = new List<string>();

        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AgentHub([NotNull] IMediator mediator, ILogger logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger;
        }

        public ImmutableArray<string> AgentIds => _agentIds.ToImmutableArray();

        public override Task OnConnectedAsync()
        {
            _logger.Debug("SignalR Agent client connected, user {User}",this.Context.User);

            return base.OnConnectedAsync();
        }

        [PublicAPI]
        public async Task AgentConnect()
        {
            var agentId = Context.UserIdentifier;

            if (string.IsNullOrWhiteSpace(agentId))
            {
                return;
            }

            _agentIds.Add(agentId);
        }

        [PublicAPI]
        public async Task DeployFailed(string deploymentTaskId, string deploymentTargetId)
        {
            _logger.Error("Deploy failed for deployment task id {DeploymentTaskId}", deploymentTaskId);
            await _mediator.Publish(new AgentDeploymentFailedNotification(deploymentTaskId, deploymentTargetId));
        }

        [PublicAPI]
        public async Task DeploySucceeded(string deploymentTaskId, string deploymentTargetId)
        {
            _logger.Information("Deploy succeeded for deployment task id {DeploymentTaskId}", deploymentTaskId);
            await _mediator.Publish(new AgentDeploymentDoneNotification(deploymentTaskId, deploymentTargetId));

        }
    }

    public class AgentDeploymentDoneNotification : INotification
    {
        public string DeploymentTaskId { get; }
        public string DeploymentTargetId { get; }

        public AgentDeploymentDoneNotification(string deploymentTaskId, string deploymentTargetId)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
        }
    }

    public class AgentDeploymentFailedNotification : INotification
    {
        public string DeploymentTaskId { get; }
        public string DeploymentTargetId { get; }

        public AgentDeploymentFailedNotification(string deploymentTaskId, string deploymentTargetId)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
        }
    }
}