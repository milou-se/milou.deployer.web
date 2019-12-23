using System;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core.DependencyInjection
{
    public class ExtendedServiceDescriptor : ServiceDescriptor
    {
        public ExtendedServiceDescriptor(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime,
            Type moduleType) : base(serviceType, implementationType, lifetime)
        {
            ModuleType = moduleType;
        }

        public ExtendedServiceDescriptor(Type serviceType, object instance, Type moduleType) : base(serviceType,
            instance)
        {
            ModuleType = moduleType;
        }

        public ExtendedServiceDescriptor(
            Type serviceType,
            Func<IServiceProvider, object> factory,
            ServiceLifetime lifetime,
            Type moduleType) : base(serviceType, factory, lifetime)
        {
            ModuleType = moduleType;
        }

        public Type ModuleType { get; }
    }
}