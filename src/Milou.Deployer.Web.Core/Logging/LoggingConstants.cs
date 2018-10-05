using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class LoggingConstants
    {
        internal const string SerilogBaseUrn = "urn:milou:deployer:web:logging:serilog";

        [Metadata(defaultValue:"false")]
        public const string SerilogDebugConsoleEnabled = SerilogBaseUrn + ":default:debug-console-enabled";

        public const string SerilogStartupLogFilePath = SerilogBaseUrn + ":startup-log-file-path";

        [Metadata(defaultValue: "false")]
        public const string SerilogSeqEnabledDefault = SerilogBaseUrn + ":default:SeqEnabled";

        [Metadata]
        public static string SerilogStartupLogEnabled = SerilogBaseUrn + ":startup-log-file:enabled";
    }
}