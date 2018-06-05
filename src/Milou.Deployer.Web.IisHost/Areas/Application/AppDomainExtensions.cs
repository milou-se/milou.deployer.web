using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyModel;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class AppDomainExtensions
    {
        private static ImmutableArray<Assembly> _cache;

        private static void ForceLoadReferenceAssemblies()
        {
            Type[] types =
            {
                typeof(ITime),
                typeof(DeployController)
            };

            foreach (Type type in types)
            {
                Console.WriteLine(type.Assembly.GetName().Name);
            }
        }
        public static ImmutableArray<Assembly> FilteredAssemblies(
            [NotNull] this AppDomain appDomain,
            string assemblyNameStartsWith = null,
            bool useCache = true)
        {
            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(appDomain));
            }

            if (useCache && !_cache.IsDefaultOrEmpty)
            {
                return _cache;
            }

            ForceLoadReferenceAssemblies();

            string[] whiteListed = { assemblyNameStartsWith ?? "milou" };

            ImmutableArray<RuntimeLibrary> defaultRuntimeLibraries =
                DependencyContext.Default?.RuntimeLibraries?.ToImmutableArray() ?? ImmutableArray<RuntimeLibrary>.Empty;

            RuntimeLibrary[] whiteListedLibraries = defaultRuntimeLibraries
                .Where(item =>
                    whiteListed.Any(listed => item.Name.StartsWith(listed, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            Assembly[] loadedAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Where(AssemblyFilter.FilterAssemblies)
                .ToArray();

            AssemblyName[] loadedAssemblyNames = loadedAssemblies
                .Select(assembly => assembly.GetName())
                .ToArray();

            RuntimeLibrary[] toLoad = whiteListedLibraries
                .Where(lib =>
                    !loadedAssemblyNames.Any(loaded => loaded.Name.Equals(lib.Name, StringComparison.Ordinal)))
                .ToArray();

            AssemblyName[] assemblyNames = toLoad
                .SelectMany(lib => lib.GetDefaultAssemblyNames(DependencyContext.Default))
                .ToArray();

            foreach (AssemblyName assemblyName in assemblyNames)
            {
                if (!AppDomain.CurrentDomain.GetAssemblies()
                    .Any(assembly => !assembly.IsDynamic && assembly.GetName().FullName == assemblyName.FullName))
                {
                    AppDomain.CurrentDomain.Load(assemblyName);
                }
            }

            var orders = new List<(string, int)>
            {
                ("test", 1000),
                ("debug", 2000)
            };

            int GetAssemblyLoadOrder(Assembly assembly)
            {
                string assemblyName = assembly.GetName().Name;

                foreach ((string Name, int Order) valueTuple in orders)
                {
                    if (assemblyName.IndexOf(valueTuple.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return valueTuple.Order;
                    }
                }

                int defaultOrder = 0;

                return defaultOrder;
            }

            ImmutableArray<Assembly> filteredAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic && whiteListed.Any(listed =>
                                       assembly.FullName.StartsWith(listed, StringComparison.OrdinalIgnoreCase)))
                .Select(assembly =>
                {
                    (Assembly Assembly, int Order) tuple = (assembly, GetAssemblyLoadOrder(assembly));

                    return tuple;
                })
                .OrderBy(tuple => tuple.Order)
                .Select(tuple => tuple.Assembly)
                .ToImmutableArray();

            if (useCache)
            {
                _cache = filteredAssemblies;
            }

            return filteredAssemblies;
        }
    }
}