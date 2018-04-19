using System;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class StringExtensions
    {
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

        public static string Wrap(this string wrappedText, string wrapText)
        {
            return $"{wrapText}{wrappedText}{wrapText}";
        }

        public static string WithDefault(this string text, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return defaultValue;
            }

            return text;
        }

        public static bool HasValue(this string text) => !string.IsNullOrWhiteSpace(text);

        public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);
    }
}