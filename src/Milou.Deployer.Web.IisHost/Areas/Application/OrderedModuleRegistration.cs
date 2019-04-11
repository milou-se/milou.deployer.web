using Autofac.Core;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public class OrderedModuleRegistration
    {
        public OrderedModuleRegistration(IModuleRegistration moduleRegistration, IModule module)
        {
            ModuleRegistration = moduleRegistration;
            Module = module;
        }

        public IModuleRegistration ModuleRegistration { get; }

        public IModule Module { get; }
    }
}
