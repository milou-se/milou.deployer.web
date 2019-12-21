using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    public interface IPackageWebHook
    {
        Task<PackageWebHookNotification> TryGetWebHookNotification(
            HttpRequest request,
            string content,
            CancellationToken cancellationToken);
    }
}