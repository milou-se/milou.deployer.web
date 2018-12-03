using Arbor.KVConfiguration.Core.Metadata;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class LoggingConstants
    {
        internal const string SerilogBaseUrn = "urn:milou:deployer:web:logging:serilog";

        [PublicAPI]
        [Metadata(defaultValue:"false")]
        public const string SerilogDebugConsoleEnabled = SerilogBaseUrn + ":default:debug-console-enabled";

        public const string SerilogStartupLogFilePath = SerilogBaseUrn + ":startup-log-file-path";

        [PublicAPI]
        [Metadata(defaultValue: "false")]
        public const string SerilogSeqEnabledDefault = SerilogBaseUrn + ":default:SeqEnabled";

        [Metadata]
        public static string SerilogStartupLogEnabled = SerilogBaseUrn + ":startup-log-file:enabled";

        [Metadata]
        public const string SeqStartupUrl = "urn:milou:deployer:web:logging:startup-seq-url";

        [Metadata]
        public const string MicrosoftLevel = "urn:milou:deployer:web:logging:microsoft:level";
    }
}