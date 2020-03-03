using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using MediatR;

using Microsoft.AspNetCore.SignalR;

using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.NuGet;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    [UsedImplicitly]
    public class PackageWebHookHubHandler : INotificationHandler<PackageEventNotification>
    {
        private readonly IHubContext<TargetHub> _targetHubContext;

        private readonly IDeploymentTargetReadService _readService;

        public PackageWebHookHubHandler(IHubContext<TargetHub> targetHubContext, IDeploymentTargetReadService readService)
        {
            _targetHubContext = targetHubContext;
            _readService = readService;
        }

        public async Task Handle(PackageEventNotification notification, CancellationToken cancellationToken)
        {
            var deploymentTargets = await _readService.GetDeploymentTargetsAsync(stoppingToken: cancellationToken);

            var targetsMatchingPackage = deploymentTargets
                .Where(
                    target => target.PackageId.Equals(
                        notification.PackageVersion.PackageId,
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (targetsMatchingPackage.Length == 0)
            {
                return;
            }

            var clientProxy = _targetHubContext.Clients.All;

            await clientProxy.SendAsync(TargetHub.TargetsWithUpdates, notification.PackageVersion.PackageId, notification.PackageVersion.Version.ToNormalizedString(), targetsMatchingPackage.Select(target => target.Id).ToArray(), cancellationToken);
        }
    }
}