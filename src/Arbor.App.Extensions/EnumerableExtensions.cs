using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Arbor.App.Extensions
{
    [PublicAPI]
    public static class EnumerableExtensions
    {
        public static ImmutableArray<T> ThrowIfDefault<T>(this ImmutableArray<T> array)
        {
            if (array.IsDefault)
            {
                throw new InvalidOperationException($"The immutable array of {typeof(T).FullName} must not be default");
            }

            return array;
        }

        public static IReadOnlyCollection<T> SafeToReadOnlyCollection<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                return ImmutableArray<T>.Empty;
            }

            if (items is IList<T> list)
            {
                return new ReadOnlyCollection<T>(list);
            }

            if (items is IReadOnlyCollection<T> readOnly)
            {
                return readOnly;
            }

            return new ReadOnlyCollection<T>(new List<T>(items));
        }

        public static ImmutableArray<T> SafeToImmutableArray<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                return ImmutableArray<T>.Empty;
            }

            if (items is ImmutableArray<T> immutable)
            {
                return immutable;
            }

            return items.ToImmutableArray();
        }

        public static IReadOnlyCollection<T> AddDefaultItemIfEmpty<T>(this IReadOnlyCollection<T> items)
        {
            if (items is null)
            {
                return new List<T> {default};
            }

            if (items.Count == 0)
            {
                return new List<T> {default};
            }

            return items;
        }
    }
}