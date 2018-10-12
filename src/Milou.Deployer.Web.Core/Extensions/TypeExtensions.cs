using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Extensions
{
    [PublicAPI]
    public static class TypeExtensions
    {
        public static bool IsConcreteTypeImplementing<T>(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsAbstract)
            {
                return false;
            }

            if (!type.IsClass)
            {
                return false;
            }

            if (!typeof(T).IsAssignableFrom(type))
            {
                return false;
            }

            if (!type.IsPublic)
            {
                return false;
            }

            return true;
        }

        public static bool IsPublicClassWithDefaultConstructor(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (!type.IsClass)
            {
                return false;
            }

            if (type.IsAbstract)
            {
                return false;
            }

            if (!type.IsPublic)
            {
                return false;
            }

            bool isInstantiatable = type.GetConstructor(Type.EmptyTypes) != null;

            return isInstantiatable;
        }

        public static ImmutableArray<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.GetTypes().ToImmutableArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null).ToImmutableArray();
            }
        }
    }
}