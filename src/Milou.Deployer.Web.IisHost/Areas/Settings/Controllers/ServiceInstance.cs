using System;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ServiceInstance
    {
        public ServiceInstance(Type registrationType, object instance, Type module)
        {
            RegistrationType = registrationType;
            Instance = instance;
            Module = module;
        }

        public Type RegistrationType { get; }
        public object Instance { get; }
        public Type Module { get; }
    }
}