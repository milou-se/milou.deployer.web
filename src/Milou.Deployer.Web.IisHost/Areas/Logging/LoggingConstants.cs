namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    public static class LoggingConstants
    {
        internal const string SerilogBaseUrn = "urn:milou:deployment:logging:serilog";

        public const string SerilogSeqUrl = SerilogBaseUrn + ":seq-url";

        public const string SerilogSeqEnabled = SerilogBaseUrn + ":seq-enabled";

        public const string SerilogStartupLogFilePath = SerilogBaseUrn + ":startup-log-file-path";
    }
}