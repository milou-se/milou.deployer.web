using Microsoft.Extensions.DependencyInjection;

namespace Arbor.App.Extensions.DependencyInjection
{
    public interface IModule
    {
        IServiceCollection Register(IServiceCollection builder);
    }
}