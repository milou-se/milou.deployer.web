using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Messages;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling
{
    [UsedImplicitly]
    public class TargetHub : Hub
    {
        public const string MessageMethod = "message";
        public const string TargetsWithUpdates = "targetsWithUpdates";
        private readonly IMediator _mediator;

        public TargetHub([NotNull] IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _mediator.Send(new UnsubscribeToDeploymentLog(Context.ConnectionId));
            await base.OnDisconnectedAsync(exception);
        }

        [PublicAPI]
        public async Task Subscribe(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            await _mediator.Send(new SubscribeToDeploymentLog(Context.ConnectionId, targetId));
        }
    }
}
