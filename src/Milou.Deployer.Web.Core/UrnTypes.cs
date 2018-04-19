using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core
{
    public static class UrnTypes
    {
        public static ImmutableArray<Type> GetUrnTypesInAppDomain()
        {
            bool HasUrnAttribute(Type type)
            {
                var customAttribute = type.GetCustomAttribute<UrnAttribute>();

                return customAttribute != null;
            }

            ImmutableArray<Type> urnMappedTypes = AppDomain.CurrentDomain.FilteredAssemblies()
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