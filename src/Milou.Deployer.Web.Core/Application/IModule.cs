using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core.Application
{
    public interface IModule
    {
        IServiceCollection Register(IServiceCollection builder);
    }
}
