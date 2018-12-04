using Arbor.KVConfiguration.Core.Metadata;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class LoggingConstants
    {
        internal const string SerilogBaseUrn = "urn:milou:deployer:web:logging:serilog";

        internal const string StartupSerilogBaseUrn = "urn:milou:deployer:web:logging:startup:serilog";

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
        public const string MicrosoftLevel = "urn:milou:deployer:web:logging:microsoft:level";

        [Metadata]
        public static string SerilogStartupLogEnabled = StartupSerilogBaseUrn + ":startup-log-file:enabled";
    }
}