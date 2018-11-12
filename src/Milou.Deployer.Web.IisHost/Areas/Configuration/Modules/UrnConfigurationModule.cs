using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Validation;
using Serilog;
using Module = Autofac.Module;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class UrnConfigurationModule : Module
    {
        private readonly ImmutableArray<Assembly> _assemblies;
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;
        private ConfigurationHolder _configurationHolder;

        public UrnConfigurationModule(
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ILogger logger,
            ImmutableArray<Assembly> assemblies)
        {
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assemblies = assemblies.ThrowIfDefault();
        }

        protected override void Load(ContainerBuilder builder)
        {
            _configurationHolder = new ConfigurationHolder();

            builder
                .RegisterInstance(_configurationHolder)
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<UserConfigUpdater>()
                .AsSelf()
                .SingleInstance();

            ImmutableArray<Type> urnMappedTypes = UrnTypes.GetUrnTypesInAssemblies(_assemblies);

            if (!bool.TryParse(_keyValueConfiguration[UrnConfigurationConstants.TreatWarningsAsErrors],
                out bool treatWarningsAsErrors))
            {
                treatWarningsAsErrors = false;
            }

            var exceptions = new List<Exception>();

            foreach (Type urnMappedType in urnMappedTypes)
            {
                try
                {
                    Register(builder, urnMappedType, treatWarningsAsErrors);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    exceptions.Add(ex);
                    _logger.Error(ex, "Could not register URN-mapped type {Type}", urnMappedType);
                }
            }

            if (exceptions.Any())
            {
                if (treatWarningsAsErrors)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        private void Register(ContainerBuilder builder, Type type, bool treatWarningsAsErrors)
        {
            ImmutableArray<INamedInstance<object>> instances = _keyValueConfiguration.GetNamedInstances(type);

            if (instances.IsDefaultOrEmpty)
            {
                var optionalAttribute = type.GetCustomAttribute<OptionalAttribute>();

                if (optionalAttribute != null)
                {
                    return;
                }

                if (treatWarningsAsErrors)
                {
                    throw new Core.DeployerAppException($"Could not get any instance of type {type.FullName}");
                }

                _logger.Warning("Could not get any instance of type {Type}", type);
                return;
            }

            ImmutableArray<INamedInstance<IValidationObject>> validationObjects = instances
                .OfType<INamedInstance<IValidationObject>>()
                .Where(item => item != null)
                .ToImmutableArray();

            if (!validationObjects.IsDefaultOrEmpty && validationObjects.Length > 0
                && !validationObjects.Any(validatedObject => validatedObject.Value.IsValid))
            {
                _logger.Warning("There are [{ValidationObjectCount}] but no valid instance of type {Type}",
                    validationObjects.Length,
                    type.FullName);

                if (treatWarningsAsErrors)
                {
                    throw new Core.DeployerAppException(
                        $"Could not create instance of type {type.FullName}, the instance is invalid, using configuration chain {(_keyValueConfiguration as MultiSourceKeyValueConfiguration)?.SourceChain}");
                }
            }

            ImmutableArray<INamedInstance<IValidationObject>> validInstances = validationObjects
                .Where(validationObject => validationObject.Value.IsValid)
                .ToImmutableArray();

            if (!validInstances.IsDefaultOrEmpty && validInstances.Length == 1)
            {
                INamedInstance<IValidationObject> validationObject = validInstances.Single();

                _logger.Debug("Registering URN-bound instance {Instance}, {Type}",
                    validationObject,
                    validationObject.GetType().FullName);

                _configurationHolder.Add(validationObject);

                builder
                    .Register(context =>
                        _configurationHolder.Get(validationObject.Value.GetType(), validationObject.Name))
                    .As(validationObject.Value.GetType());
            }
            else if (!validInstances.IsDefaultOrEmpty && validInstances.Length > 1)
            {
                foreach (INamedInstance<IValidationObject> validationObject in validInstances)
                {
                    _logger.Debug("Registering URN-bound instance {Instance}, {Type}",
                        validationObject,
                        validationObject.GetType().FullName);

                    _configurationHolder.Add(validationObject);

                    builder
                        .Register(context =>
                            _configurationHolder.Get(validationObject.Value.GetType(), validationObject.Name))
                        .As(validationObject.Value.GetType());
                }
            }
            else if (!validationObjects.IsDefaultOrEmpty)
            {
                foreach (INamedInstance<IValidationObject> validationObject in validationObjects)
                {
                    _logger.Debug("Registering invalid URN-bound instance {Instance}, {Type}",
                        validationObject,
                        validationObject.GetType().FullName);

                    _configurationHolder.Add(validationObject);

                    builder
                        .Register(context =>
                            _configurationHolder.Get(validationObject.Value.GetType(), validationObject.Name))
                        .As(validationObject.Value.GetType());
                }
            }
            else
            {
                foreach (INamedInstance<object> instance in instances)
                {
                    _logger.Debug("Registering URN-bound instance {Instance}, {Type}",
                        instance,
                        instance.GetType().FullName);
                    _configurationHolder.Add(instance);

                    builder
                        .Register(context => _configurationHolder.Get(instance.Value.GetType(), instance.Name))
                        .As(instance.Value.GetType());
                }
            }
        }
    }
}