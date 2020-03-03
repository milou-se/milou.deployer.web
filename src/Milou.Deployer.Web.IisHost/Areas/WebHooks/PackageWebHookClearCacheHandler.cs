using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using MediatR;
using Milou.Deployer.Web.Core.NuGet;
using Milou.Deployer.Web.IisHost.Areas.NuGet;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    [UsedImplicitly]
    public class PackageWebHookClearCacheHandler : INotificationHandler<PackageEventNotification>
    {
        private readonly PackageService _packageService;

        public PackageWebHookClearCacheHandler(PackageService packageService) => _packageService = packageService;

        public Task Handle(PackageEventNotification notification, CancellationToken cancellationToken)
        {
            _packageService.ClearCache(
                notification.PackageVersion.Key,
                notification.NugetConfig,
                notification.NugetSource);

            return Task.CompletedTask;
        }
    }
}