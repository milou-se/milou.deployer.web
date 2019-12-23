using System;
using System.Linq;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Configuration
{
    public class ConfigurationKeyInfo
    {
        public ConfigurationKeyInfo([NotNull] string key, [CanBeNull] string value, [CanBeNull] string source)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            Key = key;
            Value = value.MakeAnonymous(key, ApplicationStringExtensions.DefaultAnonymousKeyWords.ToArray());
            Source = source;
        }

        public string Key { get; }

        public string Value { get; }

        public string Source { get; }

        public override string ToString() =>
            $"{nameof(Key)}: {Key}, {nameof(Value)}: '{Value}', {nameof(Source)}: {Source}";
    }
}