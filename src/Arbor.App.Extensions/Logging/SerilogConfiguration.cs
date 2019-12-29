using System;
using Arbor.App.Extensions.Configuration;
using Arbor.App.Extensions.Validation;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Logging
{
    [Urn(LoggingConstants.SerilogBaseUrn)]
    [UsedImplicitly]
    public class SerilogConfiguration : IValidationObject, IConfigurationValues
    {
        public SerilogConfiguration(
            string seqUrl,
            string rollingLogFilePath,
            bool seqEnabled = false,
            bool rollingLogFilePathEnabled = false,
            bool consoleEnabled = false,
            bool debugConsoleEnabled = false)
        {
            Uri? uri = null;
            if (!seqEnabled)
            {
                IsValid = true;
            }
            else if (Uri.TryCreate(seqUrl, UriKind.Absolute, out var foundUri))
            {
                uri = foundUri;
                IsValid = true;
            }
            else
            {
                IsValid = false;
            }

            SeqUrl = uri;
            RollingLogFilePath = rollingLogFilePath;
            SeqEnabled = seqEnabled;
            RollingLogFilePathEnabled = rollingLogFilePathEnabled;
            ConsoleEnabled = consoleEnabled;
            DebugConsoleEnabled = debugConsoleEnabled;
        }

        public bool SeqEnabled { get; }

        public bool RollingLogFilePathEnabled { get; }

        [PublicAPI]
        public bool ConsoleEnabled { get; }

        public bool DebugConsoleEnabled { get; }

        public Uri? SeqUrl { get; }

        public string? RollingLogFilePath { get; }

        public bool IsValid { get; }

        public override string ToString() =>
            $"{nameof(SeqEnabled)}: {SeqEnabled}, {nameof(RollingLogFilePathEnabled)}: {RollingLogFilePathEnabled}, {nameof(ConsoleEnabled)}: {ConsoleEnabled}, {nameof(DebugConsoleEnabled)}: {DebugConsoleEnabled}, {nameof(SeqUrl)}: {SeqUrl}, {nameof(RollingLogFilePath)}: {RollingLogFilePath}, {nameof(IsValid)}: {IsValid}";
    }
}