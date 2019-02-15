using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    [UsedImplicitly]
    public class DeploymentHubLogHandler : INotificationHandler<DeploymentLogNotification>
    {
        private readonly IHubContext<DeploymentLoggingHub> _hubContext;

        public DeploymentHubLogHandler([NotNull] IHubContext<DeploymentLoggingHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task Handle(DeploymentLogNotification notification, CancellationToken cancellationToken)
        {
            ImmutableHashSet<string> tryGetTargetSubscribers =
                DeploymentLogSubscriptionHandler.TryGetTargetSubscribers(notification.DeploymentTargetId);

            if (tryGetTargetSubscribers.Count == 0)
            {
                return;
            }

            string[] clients = tryGetTargetSubscribers.ToArray();
            IClientProxy clientProxy = _hubContext.Clients.Clients(clients);

            await clientProxy.SendAsync(DeploymentLoggingHub.MessageMethod, notification.Message, cancellationToken);
        }
    }
}