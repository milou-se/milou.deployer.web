using Microsoft.Extensions.Hosting;

namespace Milou.Deployer.Web.Core.Startup
{
    public interface IStartupTask : IHostedService
    {
        bool IsCompleted { get; }
    }
}
