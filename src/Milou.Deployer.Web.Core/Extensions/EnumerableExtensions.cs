using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IReadOnlyCollection<T> SafeToReadOnlyCollection<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                return new ReadOnlyCollection<T>(new List<T>(1));
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
    }
}