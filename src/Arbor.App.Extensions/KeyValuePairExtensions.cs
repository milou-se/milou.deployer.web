using System.Collections.Generic;

namespace Arbor.App.Extensions
{
    public static class KeyValuePairExtensions
    {
        public static KeyValuePair<string, string> MakeAnonymousValue(this KeyValuePair<string, string> pair)
        {
            return new KeyValuePair<string, string>(pair.Key, new string('*', 5));
        }

        public static string ValueOrDefault(this IReadOnlyDictionary<string, string> dictionary, string key)
        {
            if (dictionary is null)
            {
                return default;
            }

            var found = dictionary.TryGetValue(key, out var value);

            if (!found)
            {
                return default;
            }

            return value;
        }
    }
}
