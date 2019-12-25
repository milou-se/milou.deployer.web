using Arbor.AspNetCore.Host.Hosting;

namespace Arbor.AspNetCore.Host
{
    public interface IServiceProviderModule
    {
        void Register(ServiceProviderHolder serviceProviderHolder);
    }
}