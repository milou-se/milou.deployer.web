using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Core;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class Bootstrapper
    {
        private static void RegisterScannedModules(
            IKeyValueConfiguration configuration,
            IReadOnlyList<IModule> modulesToRegister,
            ILogger logger,
            ImmutableArray<Assembly> assembliesToScan,
            IReadOnlyList<Type> excludedModuleTypes,
            string scopeName,
            ContainerBuilder appScopeBuilder)
        {
            var existingTypes = modulesToRegister
                .Select(item => item.GetType())
                .ToArray();

            var allExcludedTypes = excludedModuleTypes.Concat(existingTypes).ToArray();

            var orderedModuleRegistrations =
                ModuleExtensions.GetModules(assembliesToScan, allExcludedTypes, configuration);

            if (logger.IsEnabled(LogEventLevel.Verbose))
            {
                logger.Verbose(
                    "Registering module types {Types} from assemblies {Assemblies}, excluded modules: {Excluded}",
                    orderedModuleRegistrations.Select(type => type.ModuleRegistration.ModuleType.FullName)
                        .ToArray(),
                    assembliesToScan.Select(assembly => $"{assembly.FullName} {assembly.Location}").ToArray(),
                    allExcludedTypes.Select(type =>
                            $"{type.FullName}, {type.Assembly.FullName} {type.Assembly.Location}")
                        .ToArray());
            }

            foreach (var module in orderedModuleRegistrations)
            {
                appScopeBuilder.RegisterInstance(module);

                if (module.ModuleRegistration.Tag is null || module.ModuleRegistration.RegisterInRootScope)
                {
                    module.Module.RegisterModule(scopeName, appScopeBuilder, logger);
                }
            }
        }

        private static void GuardArgs(
            IReadOnlyList<IModule> modulesToRegister,
            ILogger logger,
            ImmutableArray<Assembly> assembliesToScan,
            IReadOnlyList<Type> excludedModuleTypes,
            IReadOnlyCollection<object> singletons)
        {
            if (modulesToRegister == null)
            {
                throw new ArgumentNullException(nameof(modulesToRegister));
            }

            assembliesToScan.ThrowIfDefault();

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (excludedModuleTypes == null)
            {
                throw new ArgumentNullException(nameof(excludedModuleTypes));
            }

            if (singletons == null)
            {
                throw new ArgumentNullException(nameof(singletons));
            }
        }

        private static Scope CreateRootScope(ContainerBuilder builder)
        {
            var rootScope = new Scope(Scope.RootScopeName);

            builder.RegisterInstance(rootScope);

            var container = builder.Build();

            rootScope.Lifetime = container;
            return rootScope;
        }

        private static void RegisterSingletons(IReadOnlyCollection<object> singletons, ContainerBuilder builder)
        {
            foreach (var singleton in singletons)
            {
                builder
                    .RegisterInstance(singleton)
                    .AsSelf()
                    .SingleInstance();
            }
        }

        private static void RegisterModules(
            IReadOnlyList<IModule> modulesToRegister,
            ILogger logger,
            ContainerBuilder builder)
        {
            foreach (var module in modulesToRegister)
            {
                if (logger.IsEnabled(LogEventLevel.Verbose))
                {
                    var type = module.GetType();

                    logger.Verbose("Registering pre-initialized module {Module} in container builder",
                        $"{type.FullName} assembly {type.Assembly.FullName} at {type.Assembly.Location}");
                }

                module.RegisterModule(Scope.RootScopeName, builder, logger);
            }

            if (logger.IsEnabled(LogEventLevel.Debug))
            {
                logger.Debug("Done running configuration modules");
            }
        }

        public static Scope Start(
            IKeyValueConfiguration configuration,
            [NotNull] IReadOnlyList<IModule> modulesToRegister,
            [NotNull] ILogger logger,
            ImmutableArray<Assembly> assembliesToScan,
            [NotNull] IReadOnlyList<Type> excludedModuleTypes,
            [NotNull] IReadOnlyCollection<object> singletons)
        {
            GuardArgs(modulesToRegister, logger, assembliesToScan, excludedModuleTypes, singletons);

            var builder = new ContainerBuilder();

            RegisterSingletons(singletons, builder);

            RegisterModules(modulesToRegister, logger, builder);

            var rootScope = CreateRootScope(builder);

            var appRootScope = rootScope.Lifetime.BeginLifetimeScope(Scope.AppRootScopeName,
                appScopeBuilder =>
                    RegisterScannedModules(
                        configuration,
                        modulesToRegister,
                        logger,
                        assembliesToScan,
                        excludedModuleTypes,
                        Scope.AppRootScopeName,
                        appScopeBuilder));

            rootScope.SubScope = new Scope(Scope.AppRootScopeName, appRootScope);

            return rootScope;
        }
    }
}
