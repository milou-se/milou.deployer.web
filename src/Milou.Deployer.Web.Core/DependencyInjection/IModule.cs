using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core.DependencyInjection
{
    public interface IModule
    {
        IServiceCollection Register(IServiceCollection builder);
    }
}
