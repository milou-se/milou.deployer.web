using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    [UsedImplicitly]
    public class DeploymentLogSubscriptionHandler :
        IRequestHandler<SubscribeToDeploymentLog>,
        IRequestHandler<UnsubscribeToDeploymentLog>
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _TargetMapping =
            new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        public static ImmutableHashSet<string> TryGetTargetSubscribers([NotNull] string deploymentTargetId)
        {
            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            bool tryGetTargetSubscribers =
                _TargetMapping.TryGetValue(deploymentTargetId, out HashSet<string> subscribers);

            if (!tryGetTargetSubscribers)
            {
                return ImmutableHashSet<string>.Empty;
            }

            return subscribers.ToImmutableHashSet(StringComparer.Ordinal);
        }

        public Task<Unit> Handle(SubscribeToDeploymentLog request, CancellationToken cancellationToken)
        {
            if (_TargetMapping.TryGetValue(request.DeploymentTargetId, out HashSet<string> subscribers))
            {
                subscribers.Add(request.ConnectionId);
            }
            else
            {
                _TargetMapping.TryAdd(request.DeploymentTargetId,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { request.ConnectionId });
            }

            return Task.FromResult(Unit.Value);
        }

        public Task<Unit> Handle(UnsubscribeToDeploymentLog request, CancellationToken cancellationToken)
        {
            HashSet<string>[] hashSets = _TargetMapping
                .Where(pair => pair.Value.Contains(request.ConnectionId))
                .Select(pair => pair.Value)
                .ToArray();

            foreach (HashSet<string> hashSet in hashSets)
            {
                hashSet.Remove(request.ConnectionId);
            }

            return Task.FromResult(Unit.Value);
        }
    }
}