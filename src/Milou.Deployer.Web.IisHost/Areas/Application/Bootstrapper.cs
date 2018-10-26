using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class Bootstrapper
    {
        public static AppContainerScope Start(
            string basePath,
            string contentBasePath,
            [NotNull] IReadOnlyList<IModule> modulesToRegister,
            [NotNull] ILogger logger,
            ImmutableArray<Assembly> scanAssemblies,
            [NotNull] IReadOnlyList<Type> excludedModules,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch)
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

            if (loggingLevelSwitch == null)
            {
                throw new ArgumentNullException(nameof(loggingLevelSwitch));
            }

            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(loggingLevelSwitch)
                .AsSelf()
                .SingleInstance();

            builder
                .Register(context => new EnvironmentConfiguration
            {
                ApplicationBasePath = basePath,
                ContentBasePath = contentBasePath
            })
                .AsSelf()
                .SingleInstance();

            if (logger.IsEnabled(LogEventLevel.Verbose))
            {
                foreach (IModule module in modulesToRegister)
                {
                    Type type = module.GetType();

                    logger.Verbose("Registering pre-initialized module {Module} in container builder",
                        $"{type.FullName} assembly {type.Assembly.FullName} at {type.Assembly.Location}");
                    builder.RegisterModule(module);
                }
            }

            logger.Debug("Done running configuration modules");

            Type[] existingTypes = modulesToRegister
                .Select(item => item.GetType())
                .ToArray();

            bool HasTagAttribute(Type type)
            {
                var registrationOrderAttribute = type.GetCustomAttribute<RegistrationOrderAttribute>();

                if (registrationOrderAttribute is null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(registrationOrderAttribute.Tag))
                {
                    return false;
                }

                if (registrationOrderAttribute.ReRegisterEnabled)
                {
                    return false;
                }

                return true;
            }

            Type[] moduleTypes = scanAssemblies
                .Select(assembly =>
                    assembly.GetLoadableTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsAssignableTo<IModule>()))
                .SelectMany(types => types)
                .Except(existingTypes)
                .Except(excludedModules)
                .ToArray();

            if (logger.IsEnabled(LogEventLevel.Verbose))
            {
                logger.Verbose(
                    "Registering module types {Types} from assemblies {Assemblies}, excluded modules: {Excluded}, existing types {Existing}",
                    moduleTypes.Select(type => type.FullName).ToArray(),
                    scanAssemblies.Select(assembly => $"{assembly.FullName} {assembly.Location}").ToArray(),
                    excludedModules.Select(type => $"{type.FullName}, {type.Assembly.FullName} {type.Assembly.Location}")
                        .ToArray(),
                    existingTypes.Select(type => $"{type.FullName}, {type.Assembly.FullName} {type.Assembly.Location}")
                        .ToArray());
            }

            builder
                .RegisterAssemblyTypes(scanAssemblies.ToArray())
                .Where(moduleTypes.Contains)
                .As<IModule>();

            var rootScope = new Scope();
            builder.RegisterInstance(rootScope);

            IContainer container = builder.Build();

            rootScope.Lifetime = container;

            var allModules = container.Resolve<IReadOnlyCollection<IModule>>();

            ILifetimeScope appRootScope = container.BeginLifetimeScope(appScopeBuilder =>
            {
                ImmutableArray<IModule> modules = allModules
                    .Where(module => !HasTagAttribute(module.GetType()))
                    .Select(module =>
                    {
                        var customAttribute = module.GetType().GetCustomAttribute<RegistrationOrderAttribute>();

                        return (Module: module, Order: customAttribute?.Order ?? 0);
                    })
                    .OrderBy(moduleOrder => moduleOrder.Order)
                    .Select(moduleOrder => moduleOrder.Module)
                    .ToImmutableArray();

                foreach (IModule module in modules)
                {
                    string moduleName = module.GetType().FullName;
                    try
                    {
                        logger.Debug("Registering module {Module} in scope {Scope}",
                            moduleName,
                            nameof(AppContainerScope.AppRootScope));

                        appScopeBuilder.RegisterModule(module);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        logger.Error(ex, "Could not register module {Module}", moduleName);
                        throw new DeployerAppException($"Could not register module {moduleName}", ex);
                    }
                }
            });

            rootScope.SubScope = new Scope(appRootScope);

            return new AppContainerScope(container, rootScope);
        }
    }
}