using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Extensions
{
    [PublicAPI]
    public static class TypeExtensions
    {
        public static ImmutableArray<Type> FindPublicConcreteTypesImplementing<T>(
            this IReadOnlyCollection<Assembly> assemblies)
        {
            ImmutableArray<Type> types = assemblies
                .Select(assembly =>
                    assembly.GetLoadableTypes()
                        .Where(IsPublicConcreteTypeImplementing<T>))
                .SelectMany(assemblyTypes => assemblyTypes)
                .ToImmutableArray();

            return types;
        }

        public static bool TakesTypeInPublicCtor<T>([NotNull] this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructorInfos.Length != 1)
            {
                return false;
            }

            ParameterInfo[] parameterInfos = constructorInfos[0].GetParameters();

            if (parameterInfos.Length != 1)
            {
                return false;
            }

            return parameterInfos[0].ParameterType == typeof(T);
        }

        public static bool IsPublicConcreteTypeImplementing<T>([NotNull] this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            bool isCorrectType = IsConcreteTypeImplementing<T>(type);

            if (!isCorrectType)
            {
                return false;
            }

            return type.IsPublic;
        }

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