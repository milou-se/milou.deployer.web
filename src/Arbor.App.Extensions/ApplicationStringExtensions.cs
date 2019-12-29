using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Arbor.App.Extensions
{
    [PublicAPI]
    public static class ApplicationStringExtensions
    {
        private static readonly Lazy<ImmutableArray<string>> LazyDefaultAnonymousKeyWords =
            new Lazy<ImmutableArray<string>>(Initialize);

        public static ImmutableArray<string> DefaultAnonymousKeyWords => LazyDefaultAnonymousKeyWords.Value;

        private static ImmutableArray<string> Initialize() =>
            new[] {"password", "username", "user id", "connection-string", "connectionstring"}
                .ToImmutableArray();

        public static string MakeAnonymous(this string value, string key, params string[] keyWords)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (keyWords is null || keyWords.Length == 0)
            {
                return value;
            }

            if (keyWords.Any(keyWord => key.IndexOf(keyWord, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return new string('*', 5);
            }

            return value;
        }

        public static string MakeKeyValuePairAnonymous(this string value, params string[] keyWords)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (keyWords is null || keyWords.Length == 0)
            {
                return value;
            }

            var pairs = value.ParseValues(';', '=').Select(pair =>
            {
                if (keyWords.Any(keyWord => keyWord.IndexOf(pair.Key, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return pair.MakeAnonymousValue();
                }

                return pair;
            });

            string final = string.Join("; ", pairs.Select(pair => $"{pair.Key}={pair.Value}"));

            return final;
        }

        public static string ThrowIfEmpty(this string value, string message = "")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                throw new ArgumentNullException(nameof(value), message);
            }

            return value;
        }

        public static ImmutableArray<KeyValuePair<string, string>> ParseValues(
            this string value,
            char delimiter,
            char assignment)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ImmutableArray<KeyValuePair<string, string>>.Empty;
            }

            var pairs = value.Split(delimiter);

            var keyValuePairs = pairs
                .Select(pair =>
                {
                    var parts = pair.Split(assignment);

                    return parts.Length != 2 ? default : new KeyValuePair<string, string>(parts[0], parts[1]);
                })
                .Where(pair => pair.Key != default)
                .ToImmutableArray();

            return keyValuePairs;
        }

        public static string Wrap(this string wrappedText, string wrapText) => $"{wrapText}{wrappedText}{wrapText}";

        public static string MakeKeyValuePairAnonymous(this string value, char separator, char replacementChar)
        {
            if (!value.HasValue())
            {
                return value;
            }

            var strings = value.Split(separator);

            if (strings.Length == 0)
            {
                return value;
            }

            if (strings.Length == 1)
            {
                return new string(replacementChar, strings.Length);
            }

            string result = strings[0] + separator + string.Join(separator.ToString(CultureInfo.InvariantCulture),
                                strings.Skip(1)
                                    .Where(text => text.HasValue())
                                    .Select(text => new string(replacementChar, text.Length)));
            return result;
        }

        public static string? WithDefault(this string value, string? defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value;
        }

        public static bool AllHaveValue(params string[] values) =>
            values != null && values.All(value => !string.IsNullOrWhiteSpace(value));

        public static bool HasValue([NotNullWhen(true)] this string? text) => !string.IsNullOrWhiteSpace(text);

        public static bool IsNullOrWhiteSpace(this string? text) => string.IsNullOrWhiteSpace(text);
    }
}