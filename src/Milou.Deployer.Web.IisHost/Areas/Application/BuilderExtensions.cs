using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class BuilderExtensions
    {
        public static IServiceCollection RegisterAssemblyTypes<T>(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime) where T : class
        {
            var types = assemblies.SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsPublicConcreteTypeImplementing<T>());

            foreach (var type in types)
            {
                serviceCollection.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }

            return serviceCollection;
        }

        public static IServiceCollection RegisterAssemblyTypesAsSingletons<T>(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assemblies) where T : class
        {
            return RegisterAssemblyTypes<T>(serviceCollection, assemblies, ServiceLifetime.Singleton);
        }
    }
}
