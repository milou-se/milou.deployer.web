using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
            _logger.Debug("SignalR Agent client connected, user {User}",this.Context.User.Identity.Name);

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
    }
}