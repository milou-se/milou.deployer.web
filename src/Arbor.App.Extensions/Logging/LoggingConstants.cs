using Arbor.KVConfiguration.Core.Metadata;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Logging
{
    public static class LoggingConstants
    {
        internal const string SerilogBaseUrn = "urn:arbor:app:web:logging:serilog";

        internal const string StartupSerilogBaseUrn = "urn:arbor:app:web:logging:startup:serilog";

        [PublicAPI]
        [Metadata(defaultValue: "false")]
        public const string SerilogDebugConsoleEnabled = SerilogBaseUrn + ":default:debug-console-enabled";

        public const string SerilogStartupLogFilePath = StartupSerilogBaseUrn + ":startup-log-file-path";

        [PublicAPI]
        [Metadata(defaultValue: "false")]
        public const string SerilogSeqEnabledDefault = SerilogBaseUrn + ":default:SeqEnabled";

        [Metadata]
        public const string SeqStartupUrl = StartupSerilogBaseUrn + ":startup-seq-url";

        [Metadata]
        public const string MicrosoftLevel = "urn:arbor:app:web:logging:microsoft:level";

        [Metadata]
        public const string SerilogStartupLogEnabled = StartupSerilogBaseUrn + ":startup-log-file:enabled";
    }
}