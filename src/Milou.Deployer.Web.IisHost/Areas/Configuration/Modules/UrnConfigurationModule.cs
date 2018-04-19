using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Module = Autofac.Module;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class UrnConfigurationModule : Module
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        public UrnConfigurationModule([NotNull] IKeyValueConfiguration keyValueConfiguration)
        {
            _keyValueConfiguration = keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            ImmutableArray<Type> urnMappedTypes = UrnTypes.GetUrnTypesInAppDomain();

            if (!bool.TryParse(_keyValueConfiguration[UrnConfigurationConstants.TreatWarningsAsErrors], out bool treatWarningsAsErrors))
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
                catch (Exception ex)
                {
                    Serilog.Log.Logger.Error(ex, "Could not register URN-mapped type {Type}", urnMappedType);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any() && treatWarningsAsErrors)
            {
                throw new AggregateException(exceptions);
            }
        }

        private void Register(ContainerBuilder builder, Type type, bool treatWarningsAsErrors)
        {
            ImmutableArray<object> instances = _keyValueConfiguration.GetInstances(type);

            IValidationObject[] validationObjects = instances.Select(instance =>
                instance as IValidationObject).Where(item => item != null).ToArray();

            if (validationObjects.Length > 0 && !validationObjects.Any(validatedObject => validatedObject.IsValid))
            {
                Serilog.Log.Logger.Warning("There are [{ValidationObjectCount}] but no valid instance of type {Type}", validationObjects.Length, type.FullName);

                if (treatWarningsAsErrors)
                {
                    throw new InvalidOperationException($"Could not create instance of type {type.FullName}, the instance is invalid, using configuration chain {(_keyValueConfiguration as MultiSourceKeyValueConfiguration)?.SourceChain}");
                }
            }

            object validInstance = validationObjects.FirstOrDefault(validationObject => validationObject.IsValid);

            object usedInstance = validInstance ?? instances.FirstOrDefault();

            if (usedInstance is null)
            {
                Serilog.Log.Logger.Error("Could not register URN-mapped type {Type}, instance is null", type);

                return;
            }

            Serilog.Log.Logger.Debug("Registering URN-bound instance {Instance}, {Type}", usedInstance, usedInstance.GetType().FullName);

            builder
                .RegisterInstance(usedInstance)
                .AsSelf()
                .SingleInstance();
        }
    }
}
