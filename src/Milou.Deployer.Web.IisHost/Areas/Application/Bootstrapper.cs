using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class Bootstrapper
    {
        public static AppContainerScope Start(
            string basePathFromArg,
            [NotNull] IReadOnlyList<IModule> modulesToRegister,
            [NotNull] ILogger logger,
            ImmutableArray<Assembly> scanAssemblies,
            [NotNull] IReadOnlyList<Type> excludedModules)
        {
            if (modulesToRegister == null)
            {
                throw new ArgumentNullException(nameof(modulesToRegister));
            }

            scanAssemblies.ThrowIfDefault();

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (excludedModules == null)
            {
                throw new ArgumentNullException(nameof(excludedModules));
            }

            var builder = new ContainerBuilder();

            foreach (IModule module in modulesToRegister)
            {
                logger.Debug("Registering pre-initialized module {Module} in container builder", module.GetType().FullName);
                builder.RegisterModule(module);
            }

            logger.Debug("Done running configuration modules");

            Type[] existingTypes = modulesToRegister
                .Select(item => item.GetType())
                .ToArray();

            Type[] moduleTypes = scanAssemblies
                .Select(assembly =>
                    assembly.GetLoadableTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsAssignableTo<IModule>()))
                .SelectMany(types => types)
                .Except(existingTypes)
                .Except(excludedModules)
                .ToArray();

            builder
                .RegisterAssemblyTypes(scanAssemblies.ToArray())
                .Where(moduleTypes.Contains)
                .As<IModule>();

            IContainer container = builder.Build();

            ILifetimeScope appRootScope = container.BeginLifetimeScope(appScopeBuilder =>
            {
                ImmutableArray<IModule> modules = container.Resolve<IReadOnlyCollection<IModule>>()
                    .Select(module =>
                    {
                        var customAttribute = module.GetType().GetCustomAttribute<RegistrationOrderAttribute>();

                        return (Module: module, Order: customAttribute?.Order ?? 0);
                    })
                    .OrderBy(tuple => tuple.Order)
                    .Select(tuple => tuple.Module)
                    .ToImmutableArray();

                foreach (IModule module in modules)
                {
                    logger.Debug("Registering module {Module} in scope {Scope}", module.GetType().FullName, nameof(AppContainerScope.AppRootScope));
                    appScopeBuilder.RegisterModule(module);
                }
            });

            return new AppContainerScope(container, appRootScope);
        }
    }
}