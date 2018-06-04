using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    [UsedImplicitly]
    public class DeploymentLogHandler : INotificationHandler<DeploymentLogNotification>,
        IRequestHandler<SubscribeToDeploymentLog>,
        IRequestHandler<UnsubscribeToDeploymentLog>
    {
        private readonly IHubContext<DeploymentLoggingHub> _hubContext;

        private static readonly ConcurrentDictionary<string, HashSet<string>> _TargetMapping =
            new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        public DeploymentLogHandler([NotNull] IHubContext<DeploymentLoggingHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task Handle(DeploymentLogNotification notification, CancellationToken cancellationToken)
        {
            if (!_TargetMapping.TryGetValue(notification.DeploymentTargetId, out HashSet<string> subscribers))
            {
                return;
            }

            string[] clients = subscribers.ToArray();
            IClientProxy clientProxy = _hubContext.Clients.Clients(clients);

            await clientProxy.SendAsync(DeploymentLoggingHub.MessageMethod, notification.Message, cancellationToken);
        }

        public Task Handle(SubscribeToDeploymentLog request, CancellationToken cancellationToken)
        {
            if (_TargetMapping.TryGetValue(request.DeploymentTargetId, out HashSet<string> subscribers))
            {
                subscribers.Add(request.ConnectionId);
            }
            else
            {
                _TargetMapping.TryAdd(request.DeploymentTargetId, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {request.ConnectionId});
            }

            return Task.CompletedTask;
        }

        public Task Handle(UnsubscribeToDeploymentLog request, CancellationToken cancellationToken)
        {
            HashSet<string>[] hashSets = _TargetMapping
                .Where(pair => pair.Value.Contains(request.ConnectionId))
                .Select(pair => pair.Value)
                .ToArray();

            foreach (HashSet<string> hashSet in hashSets)
            {
                hashSet.Remove(request.ConnectionId);
            }

            return Task.CompletedTask;
        }
    }
}