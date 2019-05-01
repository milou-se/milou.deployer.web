using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.DependencyInjection;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Validation;
using Serilog;
using UrnTypes = Milou.Deployer.Web.Core.Configuration.UrnTypes;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class UrnConfigurationModule : IModule
    {
        private readonly ConfigurationInstanceHolder _configurationHolder;
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;

        public UrnConfigurationModule(
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ILogger logger,
            ConfigurationInstanceHolder configurationHolder)
        {
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationHolder = configurationHolder;
        }

        private void Register(IServiceCollection serviceCollection, Type type, bool treatWarningsAsErrors)
        {
            var instances = _keyValueConfiguration.GetNamedInstances(type);

            if (instances.IsDefaultOrEmpty)
            {
                var optionalAttribute = type.GetCustomAttribute<OptionalAttribute>();

                if (optionalAttribute != null)
                {
                    return;
                }

                if (treatWarningsAsErrors)
                {
                    serviceCollection.AddSingleton(
                        new ConfigurationError($"Could not get any instance of type {type.FullName}"));
                }

                _logger.Warning("Could not get any instance of type {Type}", type);
                return;
            }

            var validationObjects = instances
                .OfType<INamedInstance<IValidationObject>>()
                .Where(item => item != null)
                .ToImmutableArray();

            if (!validationObjects.IsDefaultOrEmpty && validationObjects.Length > 0
                                                    && !validationObjects.Any(validatedObject =>
                                                        validatedObject.Value.IsValid))
            {
                _logger.Warning("There are [{ValidationObjectCount}] but no valid instance of type {Type}",
                    validationObjects.Length,
                    type.FullName);

                if (treatWarningsAsErrors)
                {
                    var invalidInstances = string.Join(Environment.NewLine,
                        validationObjects
                            .Where(validatedObject => !validatedObject.Value.IsValid)
                            .Select(namedInstance => $"[{namedInstance.Value}] {namedInstance.Value}"));

                    serviceCollection.AddSingleton(new ConfigurationError(
                            $"Could not create instance of type {type.FullName}, the instances '{invalidInstances}' are invalid, using configuration chain {(_keyValueConfiguration as MultiSourceKeyValueConfiguration)?.SourceChain}"),
                        this);
                    return;
                }
            }

            var validInstances = validationObjects
                .Where(validationObject => validationObject.Value.IsValid)
                .ToImmutableArray();

            if (!validInstances.IsDefaultOrEmpty && validInstances.Length == 1)
            {
                var validationObject = validInstances.Single();

                var instance = $"[{validationObject.Name}] {validationObject.Value}";

                _logger.Debug("Registering URN-bound instance {Instance}, {Type}",
                    instance,
                    validationObject.GetType().FullName);

                _configurationHolder.Add(validationObject);

                serviceCollection
                    .AddSingleton(validationObject.Value.GetType(),
                        context =>
                            _configurationHolder.Get(validationObject.Value.GetType(), validationObject.Name),
                        this);
            }
            else if (!validInstances.IsDefaultOrEmpty && validInstances.Length > 1)
            {
                foreach (var validationObject in validInstances)
                {
                    _logger.Debug("Registering URN-bound instance {Instance}, {Type}",
                        $"[{validationObject.Name}] {validationObject.Value}",
                        validationObject.GetType().FullName);

                    _configurationHolder.Add(validationObject);

                    serviceCollection
                        .AddSingleton(validationObject.Value.GetType(),
                            context =>
                                _configurationHolder.Get(validationObject.Value.GetType(), validationObject.Name),
                            this);
                }
            }
            else if (!validationObjects.IsDefaultOrEmpty)
            {
                foreach (var validationObject in validationObjects)
                {
                    _logger.Debug("Registering invalid URN-bound instance {Instance}, {Type}",
                        $"[{validationObject.Name}] {validationObject.Value}",
                        validationObject.GetType().FullName);

                    _configurationHolder.Add(validationObject);

                    serviceCollection
                        .AddSingleton(validationObject.Value.GetType(),
                            context =>
                                _configurationHolder.Get(validationObject.Value.GetType(), validationObject.Name),
                            this);
                }
            }
            else
            {
                foreach (var instance in instances)
                {
                    _logger.Debug("Registering URN-bound instance {Instance}, {Type}",
                        $"[{instance.Name}] {instance.Value}",
                        instance.GetType().FullName);
                    _configurationHolder.Add(instance);

                    serviceCollection
                        .AddSingleton(instance.Value.GetType(),
                            context => _configurationHolder.Get(instance.Value.GetType(), instance.Name),
                            this);
                }
            }
        }

        public IServiceCollection Register(IServiceCollection builder)
        {
            builder.AddSingleton<UserConfigUpdater>();

            var urnMappedTypes = UrnTypes.GetUrnTypesInAssemblies(ApplicationAssemblies.FilteredAssemblies());

            if (!bool.TryParse(_keyValueConfiguration[UrnConfigurationConstants.TreatWarningsAsErrors],
                out var treatWarningsAsErrors))
            {
                treatWarningsAsErrors = false;
            }

            var exceptions = new List<Exception>();

            foreach (var urnMappedType in urnMappedTypes)
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

            return builder;
        }
    }
}
