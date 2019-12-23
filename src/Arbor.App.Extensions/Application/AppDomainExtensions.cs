using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.App.Extensions.Logging;
using Arbor.App.Extensions.Time;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyModel;
using Serilog;
using Serilog.Core;

namespace Arbor.App.Extensions.Application
{
    public static class AppDomainExtensions
    {
        private static ImmutableArray<Assembly> _cache;

        private static void ForceLoadReferenceAssemblies()
        {
            Type[] types = {typeof(ICustomClock)};

            foreach (var type in types)
            {
                TempLogger.WriteLine(type.Assembly.GetName().Name);
            }
        }

        public static ImmutableArray<Assembly> FilteredAssemblies(
            [NotNull] this AppDomain appDomain,
            string assemblyNameStartsWith = null,
            bool useCache = true,
            ILogger logger = null)
        {
            logger ??= Logger.None;

            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(appDomain));
            }

            if (useCache && !_cache.IsDefaultOrEmpty)
            {
                return _cache;
            }

            ForceLoadReferenceAssemblies();

            string[] allowedAssemblies = {assemblyNameStartsWith ?? "Milou.Deployer.Web"};

            try
            {
                var defaultRuntimeLibraries =
                    DependencyContext.Default?.RuntimeLibraries?.ToImmutableArray() ??
                    ImmutableArray<RuntimeLibrary>.Empty;

                var includedLibraries = defaultRuntimeLibraries
                    .Where(item =>
                        allowedAssemblies.Any(
                            listed => item.Name.StartsWith(listed, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                var loadedAssemblies = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => !assembly.IsDynamic)
                    .Where(assembly => allowedAssemblies.Any(allowed =>
                        assembly.GetName().Name.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                var loadedAssemblyNames = loadedAssemblies
                    .Select(assembly => assembly.GetName())
                    .ToArray();

                var toLoad = includedLibraries
                    .Where(lib =>
                        !loadedAssemblyNames.Any(loaded => loaded.Name.Equals(lib.Name, StringComparison.Ordinal)))
                    .ToArray();

                var assemblyNames = toLoad
                    .SelectMany(lib => lib.GetDefaultAssemblyNames(DependencyContext.Default))
                    .ToArray();

                foreach (var assemblyName in assemblyNames)
                {
                    if (!AppDomain.CurrentDomain.GetAssemblies()
                        .Any(assembly => !assembly.IsDynamic && assembly.GetName().FullName == assemblyName.FullName))
                    {
                        AppDomain.CurrentDomain.Load(assemblyName);
                    }
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                logger.Warning(ex, "Could not load runtime assemblies");
            }

            var orders = new List<(string, int)> {("test", 1000), ("debug", 2000)};

            int GetAssemblyLoadOrder(Assembly assembly)
            {
                string? assemblyName = assembly.GetName().Name;

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

            var filteredAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic && allowedAssemblies.Any(listed =>
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