using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core.DependencyInjection
{
    public static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleton(
            this IServiceCollection serviceCollection,
            Type implementationType,
            IModule module)
        {
            return serviceCollection.AddSingleton(implementationType, implementationType, module);
        }

        public static IServiceCollection AddSingleton(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Type implementationType,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(registrationType,
                implementationType,
                ServiceLifetime.Singleton,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            TRegistrationType implementation,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType), implementation, module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, TRegistrationType> resolver,
            IModule module)
        {
            object ObjectResolver(IServiceProvider provider) => resolver.Invoke(provider);

            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType), ObjectResolver, ServiceLifetime.Singleton, module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType, TImplementationType>(
            this IServiceCollection serviceCollection,
            TImplementationType implementation,
            IModule module) where TImplementationType : TRegistrationType
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                implementation,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType, TImplementationType>(
            this IServiceCollection serviceCollection,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                typeof(TImplementationType),
                ServiceLifetime.Singleton,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<T>(
            this IServiceCollection serviceCollection,
            IModule module)
        {
            return serviceCollection.AddSingleton<T, T>(module);
        }

        public static IServiceCollection Add(
            this IServiceCollection serviceCollection,
            Type implementationType,
            ServiceLifetime serviceLifetime,
            IModule module)
        {
            return serviceCollection.Add(implementationType, implementationType, serviceLifetime, module);
        }

        public static IServiceCollection Add(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Type implementationType,
            ServiceLifetime serviceLifetime,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(registrationType,
                implementationType,
                serviceLifetime,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add<TRegistrationType, TImplementationType>(
            this IServiceCollection serviceCollection,
            ServiceLifetime serviceLifetime,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                typeof(TImplementationType),
                serviceLifetime,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Func<IServiceProvider, object> resolver,
            ServiceLifetime serviceLifetime,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(registrationType,
                resolver,
                serviceLifetime,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Func<IServiceProvider, object> resolver,
            IModule module)
        {
            return serviceCollection.Add(registrationType, resolver, ServiceLifetime.Singleton, module);
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, ServiceFactory> resolver,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                resolver,
                ServiceLifetime.Singleton,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, object> resolver,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                resolver,
                ServiceLifetime.Singleton,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, object> resolver,
            ServiceLifetime serviceLifetime,
            IModule module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                resolver,
                serviceLifetime,
                module.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add<T>(
            this IServiceCollection serviceCollection,
            ServiceLifetime serviceLifetime,
            IModule module)
        {
            return serviceCollection.Add<T, T>(serviceLifetime, module);
        }
    }
}
