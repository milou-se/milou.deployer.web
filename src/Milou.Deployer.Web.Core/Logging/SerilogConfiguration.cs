using System;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Logging
{
    [Urn(LoggingConstants.SerilogBaseUrn)]
    [UsedImplicitly]
    public class SerilogConfiguration : Validation.IValidationObject
    {
        public SerilogConfiguration(
            string seqUrl,
            string rollingLogFilePath,
            bool seqEnabled = false,
            bool rollingLogFilePathEnabled = false,
            bool consoleEnabled = false,
            bool debugConsoleEnabled = false)
        {
            SeqUrl = seqUrl;
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

        public string SeqUrl { get; }

        public string RollingLogFilePath { get; }

        public bool IsValid =>
            string.IsNullOrWhiteSpace(SeqUrl) || Uri.TryCreate(SeqUrl, UriKind.Absolute, out Uri _);

        public override string ToString()
        {
            return $"{nameof(SeqEnabled)}: {SeqEnabled}, {nameof(RollingLogFilePathEnabled)}: {RollingLogFilePathEnabled}, {nameof(ConsoleEnabled)}: {ConsoleEnabled}, {nameof(DebugConsoleEnabled)}: {DebugConsoleEnabled}, {nameof(SeqUrl)}: {SeqUrl}, {nameof(RollingLogFilePath)}: {RollingLogFilePath}, {nameof(IsValid)}: {IsValid}";
        }
    }
}
