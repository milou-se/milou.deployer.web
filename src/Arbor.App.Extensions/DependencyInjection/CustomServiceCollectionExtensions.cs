using System;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.App.Extensions.DependencyInjection
{
    public static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleton(
            this IServiceCollection serviceCollection,
            Type implementationType,
            IModule? module) =>
            serviceCollection.AddSingleton(implementationType, implementationType, module);

        public static IServiceCollection AddSingleton(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Type implementationType,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(registrationType,
                implementationType,
                ServiceLifetime.Singleton,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            [NotNull] TRegistrationType implementation,
            IModule? module)
        {
            if (implementation == null)
            {
                throw new ArgumentNullException(nameof(implementation));
            }

            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType), implementation,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, TRegistrationType> resolver,
            IModule? module)
        {
            object ObjectResolver(IServiceProvider provider)
            {
                var registrationType = resolver.Invoke(provider);

                if (registrationType is null)
                {
                    throw new InvalidOperationException(
                        $"Could not resolve type with provider {provider.GetType().Name}");
                }

                return registrationType;
            }

            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType), ObjectResolver,
                ServiceLifetime.Singleton, module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType, TImplementationType>(
            this IServiceCollection serviceCollection,
            [NotNull] TImplementationType implementation,
            IModule? module) where TImplementationType : TRegistrationType
        {
            if (implementation == null)
            {
                throw new ArgumentNullException(nameof(implementation));
            }

            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                implementation,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType, TImplementationType>(
            this IServiceCollection serviceCollection,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                typeof(TImplementationType),
                ServiceLifetime.Singleton,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<T>(
            this IServiceCollection serviceCollection,
            IModule? module) =>
            serviceCollection.AddSingleton<T, T>(module);

        public static IServiceCollection Add(
            this IServiceCollection serviceCollection,
            Type implementationType,
            ServiceLifetime serviceLifetime,
            IModule? module) =>
            serviceCollection.Add(implementationType, implementationType, serviceLifetime, module);

        public static IServiceCollection Add(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Type implementationType,
            ServiceLifetime serviceLifetime,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(registrationType,
                implementationType,
                serviceLifetime,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add<TRegistrationType, TImplementationType>(
            this IServiceCollection serviceCollection,
            ServiceLifetime serviceLifetime,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                typeof(TImplementationType),
                serviceLifetime,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Func<IServiceProvider, object> resolver,
            ServiceLifetime serviceLifetime,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(registrationType,
                resolver,
                serviceLifetime,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton(
            this IServiceCollection serviceCollection,
            Type registrationType,
            Func<IServiceProvider, object> resolver,
            IModule? module) =>
            serviceCollection.Add(registrationType, resolver, ServiceLifetime.Singleton, module);

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, ServiceFactory> resolver,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                resolver,
                ServiceLifetime.Singleton,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection AddSingleton<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, object> resolver,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                resolver,
                ServiceLifetime.Singleton,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add<TRegistrationType>(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, object> resolver,
            ServiceLifetime serviceLifetime,
            IModule? module)
        {
            serviceCollection.Add(new ExtendedServiceDescriptor(typeof(TRegistrationType),
                resolver,
                serviceLifetime,
                module?.GetType()));

            return serviceCollection;
        }

        public static IServiceCollection Add<T>(
            this IServiceCollection serviceCollection,
            ServiceLifetime serviceLifetime,
            IModule? module) =>
            serviceCollection.Add<T, T>(serviceLifetime, module);
    }
}