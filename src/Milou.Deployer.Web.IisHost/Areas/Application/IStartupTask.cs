using Microsoft.Extensions.Hosting;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public interface IStartupTask : IHostedService
    {
        bool IsCompleted { get; }
    }
}
