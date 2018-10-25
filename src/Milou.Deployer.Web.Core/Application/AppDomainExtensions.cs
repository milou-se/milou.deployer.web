using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyModel;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
using Serilog;
using Serilog.Core;

namespace Milou.Deployer.Web.Core.Application
{
    public static class AppDomainExtensions
    {
        private static ImmutableArray<Assembly> _cache;

        public static ImmutableArray<Assembly> FilteredAssemblies(
            [NotNull] this AppDomain appDomain,
            string assemblyNameStartsWith = null,
            bool useCache = true,
            ILogger logger = null,
            string dllLoadPath = null)
        {
            logger = logger ?? Logger.None;

            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(appDomain));
            }

            if (useCache && !_cache.IsDefaultOrEmpty)
            {
                return _cache;
            }

            ForceLoadReferenceAssemblies();

            string[] allowedAssemblies = { assemblyNameStartsWith ?? "Milou.Deployer.Web" };

            try
            {
                var scanAssemblies = new List<FileInfo>();

                if (!string.IsNullOrWhiteSpace(dllLoadPath))
                {
                    var directoryInfo = new DirectoryInfo(dllLoadPath);

                    if (directoryInfo.Exists)
                    {
                        var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

                        FileInfo[] FilterAppAssemblies(FileInfo[] files)
                        {
                            return files
                                .Where(file => !file.Name.StartsWith("host", StringComparison.Ordinal))
                                .ToArray();
                        }

                        FileInfo[] dllFiles = FilterAppAssemblies(directoryInfo.GetFiles("*.dll"));

                        FileInfo[] existingDllFiles = baseDirectory.GetFiles("*.dll");

                        FileInfo[] existingFilesToLoad = FilterAppAssemblies(existingDllFiles);

                        scanAssemblies.AddRange(existingFilesToLoad);

                        foreach (FileInfo sourceDllFile in dllFiles)
                        {
                            if (!existingDllFiles.Any(file => file.Name.Equals(sourceDllFile.Name)))
                            {
                                var targetFile =new FileInfo(Path.Combine(baseDirectory.FullName, sourceDllFile.Name));

                                sourceDllFile.CopyTo(targetFile.FullName, false);

                                scanAssemblies.Add(targetFile);
                            }
                        }

                        foreach (FileInfo assemblyFile in scanAssemblies)
                        {
                            try
                            {
                                Assembly loadFile = Assembly.LoadFile(assemblyFile.FullName);

                                logger.Verbose("Loaded assembly {Assembly} from file {File}", loadFile.GetName(), assemblyFile.FullName);
                            }
                            catch (BadImageFormatException)
                            {
                                // ignore
                            }
                            catch (Exception ex) when (!ex.IsFatal())
                            {
                                logger.Warning(ex, "Could not load assembly from path {Path}", assemblyFile.FullName);
                            }
                        }
                    }
                }

                ImmutableArray<RuntimeLibrary> defaultRuntimeLibraries =
                    DependencyContext.Default?.RuntimeLibraries?.ToImmutableArray() ??
                    ImmutableArray<RuntimeLibrary>.Empty;

                RuntimeLibrary[] includedLibraries = defaultRuntimeLibraries
                    .Where(item =>
                        allowedAssemblies.Any(
                            listed => item.Name.StartsWith(listed, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                Assembly[] loadedAssemblies = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => !assembly.IsDynamic)
                    .Where(assembly => allowedAssemblies.Any(allowed =>
                        assembly.GetName().Name.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                AssemblyName[] loadedAssemblyNames = loadedAssemblies
                    .Select(assembly => assembly.GetName())
                    .ToArray();

                RuntimeLibrary[] toLoad = includedLibraries
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
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                logger.Warning(ex, "Could not load runtime assemblies");
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

        private static void ForceLoadReferenceAssemblies()
        {
            Type[] types =
            {
                typeof(ICustomClock)
            };

            foreach (Type type in types)
            {
                Console.WriteLine(type.Assembly.GetName().Name);
            }
        }
    }
}