using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Urns;

namespace Arbor.App.Extensions.Configuration
{
    public static class UrnTypes
    {
        public static ImmutableArray<Type> GetUrnTypesInAssemblies(ImmutableArray<Assembly> assemblies)
        {
            bool HasUrnAttribute(Type type)
            {
                var customAttribute = type.GetCustomAttribute<UrnAttribute>();

                return customAttribute != null;
            }

            var urnMappedTypes = assemblies
                .Select(assembly =>
                    assembly.GetLoadableTypes()
                        .Where(type => !type.IsAbstract && type.IsPublic)
                        .Where(HasUrnAttribute))
                .SelectMany(types => types)
                .ToImmutableArray();

            return urnMappedTypes;
        }
    }
}
