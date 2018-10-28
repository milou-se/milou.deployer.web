using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Core;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class ModuleExtensions
    {
        public static ImmutableArray<OrderedModuleRegistration> GetModules(
            IReadOnlyCollection<Assembly> assemblies,
            IReadOnlyCollection<Type> excludedTypes,
            IKeyValueConfiguration configuration)
        {
            Type[] moduleTypes = assemblies
                .FindPublicConcreteTypesImplementing<IModule>()
                .Except(excludedTypes)
                .ToArray();

            ImmutableArray<OrderedModuleRegistration> modules = moduleTypes
                .Select(moduleType => new ModuleRegistration(moduleType))
                .Select(item => CreateModuleInstance(item, configuration))
                .OrderBy(item => item.ModuleRegistration.Order)
                .ToImmutableArray();

            return modules;
        }

        public static void RegisterModule([NotNull] this IModule module, [NotNull] string scopeName,
            [NotNull] ContainerBuilder builder,
            [NotNull] ILogger logger)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (string.IsNullOrWhiteSpace(scopeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(scopeName));
            }

            string moduleName = module.GetType().FullName;

            try
            {
                logger.Debug("Registering module {Module} in scope {Scope}",
                    moduleName,
                    scopeName);

                builder.RegisterModule(module);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                logger.Error(ex, "Could not register module {Module}", moduleName);
                throw new DeployerAppException($"Could not register module {moduleName}", ex);
            }
        }

        private static OrderedModuleRegistration CreateModuleInstance(
            IModuleRegistration moduleRegistration,
            IKeyValueConfiguration keyValueConfiguration)
        {
            if (!moduleRegistration.ModuleType.IsPublicClassWithDefaultConstructor())
            {
                if (moduleRegistration.ModuleType.TakesTypeInPublicCtor<IKeyValueConfiguration>())
                {
                    if (Activator.CreateInstance(moduleRegistration.ModuleType, keyValueConfiguration) is IModule
                        moduleWithArgs)
                    {
                        return new OrderedModuleRegistration(moduleRegistration, moduleWithArgs);
                    }
                }

                throw new DeployerAppException(
                    $"Could not instantiate type module type {moduleRegistration.ModuleType.FullName}");
            }

            if (Activator.CreateInstance(moduleRegistration.ModuleType) is IModule module)
            {
                return new OrderedModuleRegistration(moduleRegistration, module);
            }

            throw new DeployerAppException(
                $"Could not instantiate type module type {moduleRegistration.ModuleType.FullName}");
        }
    }
}