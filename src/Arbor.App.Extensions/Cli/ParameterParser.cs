using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Cli
{
    public static class ParameterParser
    {
        public static string ParseParameter(
            [NotNull] this IReadOnlyCollection<string> parameters,
            [NotNull] string parameterName)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parameterName));
            }

            string trimmedName = parameterName.Trim();
            string prefix = $"{trimmedName}=";
            var matchingArgs = parameters
                .Where(param => param != null)
                .Select(param => param.Trim())
                .Where(param => param.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matchingArgs.Length == 0)
            {
                return null;
            }

            if (matchingArgs.Length > 1)
            {
                throw new InvalidOperationException($"Found more than 1 parameter named '{parameterName}'");
            }

            string value = matchingArgs[0].Substring(prefix.Length).Trim();

            return value;
        }
    }
}