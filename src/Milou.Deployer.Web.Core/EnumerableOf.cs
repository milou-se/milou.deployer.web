using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Milou.Deployer.Web.Core
{
    public static class EnumerableOf<T> where T : class
    {
        private static readonly Lazy<ImmutableArray<T>> _All = new Lazy<ImmutableArray<T>>(() => GetAll());

        public static ImmutableArray<T> All => _All.Value;

        private static ImmutableArray<T> GetAll() =>
            typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField)
                .Where(field => field.FieldType == typeof(T) && field.IsInitOnly)
                .Select(field => (T)field.GetValue(null)!)
                .ToImmutableArray();
    }
}