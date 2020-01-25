﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbor.App.Extensions;
using Arbor.App.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core
{
    public static class BuilderExtensions
    {
        public static IServiceCollection RegisterAssemblyTypes<T>(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime,
            IModule module = null) where T : class
        {
            var types = assemblies
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => type.IsPublicConcreteTypeImplementing<T>());

            foreach (var type in types)
            {
                serviceCollection.Add(new ExtendedServiceDescriptor(typeof(T), type, lifetime, module?.GetType()));
            }

            return serviceCollection;
        }

        public static IServiceCollection RegisterAssemblyTypesAsSingletons<T>(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assemblies,
            IModule module = null) where T : class =>
            RegisterAssemblyTypes<T>(serviceCollection, assemblies, ServiceLifetime.Singleton, module);
    }
}