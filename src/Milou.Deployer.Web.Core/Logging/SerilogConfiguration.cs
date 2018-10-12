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
            string startupLogFilePath,
            string rollingLogFilePath,
            bool seqEnabled = false,
            bool rollingLogFilePathEnabled = false,
            bool consoleEnabled = false,
            bool debugConsoleEnabled = false)
        {
            SeqUrl = seqUrl;
            StartupLogFilePath = startupLogFilePath;
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

        [PublicAPI]
        public string StartupLogFilePath { get; }

        public string RollingLogFilePath { get; }

        public bool IsValid =>
            string.IsNullOrWhiteSpace(SeqUrl) || Uri.TryCreate(SeqUrl, UriKind.Absolute, out Uri _);
    }
}
