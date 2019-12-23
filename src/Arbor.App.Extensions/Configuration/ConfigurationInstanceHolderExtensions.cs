using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.KVConfiguration.Urns;

namespace Arbor.App.Extensions.Configuration
{
    public static class ConfigurationInstanceHolderExtensions
    {
        public static void AddInstance<T>(this ConfigurationInstanceHolder holder, T instance) where T : class
        {
            holder.Add(new NamedInstance<T>(instance, instance.GetType().FullName));
        }

        public static T Get<T>(this ConfigurationInstanceHolder holder) where T : class
        {
            return holder.GetInstances<T>().SingleOrDefault().Value;
        }

        public static T Create<T>(this ConfigurationInstanceHolder holder) where T : class
        {
            return (T)Create(holder, typeof(T));
        }

        public static IEnumerable<T> CreateInstances<T>(this ConfigurationInstanceHolder holder) where T : class
        {
            var registeredTypes = holder.RegisteredTypes
                .Where(registered => typeof(T).IsAssignableFrom(registered) && !registered.IsAbstract)
                .ToArray();

            foreach (Type registeredType in registeredTypes)
            {
                foreach (T instance in holder.GetInstances(registeredType).Values.OfType<T>())
                {
                    yield return instance;
                }
            }
        }

        public static object Create(this ConfigurationInstanceHolder holder, Type type)
        {
            var instances = holder.GetInstances(type);

            if (instances.Count == 1)
            {
                return instances.Values.FirstOrDefault();
            }

            if (instances.Count > 1)
            {
                throw new InvalidOperationException($"Found multiple instances of type {type.FullName}");
            }

            var constructors = type.GetConstructors();

            if (constructors.Length != 1)
            {
                throw new InvalidOperationException($"The type {type.FullName} has multiple constructors");
            }

            var constructorInfo = constructors[0];

            var parameters = constructorInfo.GetParameters();

            var missingArgs = parameters.Where(p =>
                    !holder.RegisteredTypes.Any(registeredType => p.ParameterType.IsAssignableFrom(registeredType)) &&
                    !p.IsOptional)
                .ToArray();

            var optionalArgs = parameters.Where(p =>
                    !holder.RegisteredTypes.Any(registeredType => p.ParameterType.IsAssignableFrom(registeredType)) &&
                    p.IsOptional)
                .ToArray();

            if (missingArgs.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Missing types defined in ctor for type {type.FullName}: {string.Join(", ", missingArgs.Select(m => m.ParameterType.FullName))}");
            }

            var args = parameters.Length == 0
                ? Array.Empty<object>()
                : parameters.Select(p => optionalArgs.Contains(p)
                    ? null
                    : holder.GetInstances(holder.RegisteredTypes.Single(reg => p.ParameterType.IsAssignableFrom(reg)))
                        .Single().Value).ToArray();

            return Activator.CreateInstance(type, args);
        }
    }
}
