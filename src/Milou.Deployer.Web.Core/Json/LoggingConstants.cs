namespace Milou.Deployer.Web.Core.Json
{
    public static class LoggingConstants
    {
        private const string SerilogBaseUrn = "urn:milou:deployment:logging:serilog";

        public const string SerilogSeqUrl = SerilogBaseUrn + ":seq-url";

        public const string SerilogSeqEnabled = SerilogBaseUrn + ":seq-enabled";

        public const string SerilogStartupLogFilePath = SerilogBaseUrn + ":startup-log-file-path";
    }
}
