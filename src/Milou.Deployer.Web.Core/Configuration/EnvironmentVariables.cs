using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

namespace Milou.Deployer.Web.Core.Configuration
{
    public static class EnvironmentVariables
    {
        public static ImmutableDictionary<string, string> Get()
        {
            return Environment.GetEnvironmentVariables()
                .OfType<DictionaryEntry>()
                .ToImmutableDictionary(entry => (string)entry.Key, entry => (string)entry.Value);
        }
    }
}