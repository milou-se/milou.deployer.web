using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    public class ConfigurationHolder
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _configurationInstances =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();

        public ImmutableArray<Type> RegisteredTypes => _configurationInstances.Keys.ToImmutableArray();

        public object Get(Type type, string key)
        {
            if (!_configurationInstances.TryGetValue(type, out var instances))
            {
                return ImmutableArray<object>.Empty;
            }

            instances.TryGetValue(key, out var instance);

            return instance;
        }

        public void Add([NotNull] INamedInstance<object> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!_configurationInstances.TryGetValue(instance.Value.GetType(),
                out var bag))
            {
                var typeDictionary = new ConcurrentDictionary<string, object>();
                typeDictionary.AddOrUpdate(instance.Name, instance.Value, (name, found) => instance.Value);

                _configurationInstances.TryAdd(instance.Value.GetType(), typeDictionary);
            }
            else
            {
                bag.AddOrUpdate(instance.Name, instance.Value, (name, found) => instance.Value);
            }
        }
    }
}
