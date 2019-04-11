using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    [UsedImplicitly]
    public class DeploymentLoggingHub : Hub
    {
        public const string MessageMethod = "message";
        private readonly IMediator _mediator;

        public DeploymentLoggingHub([NotNull] IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

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
