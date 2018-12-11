namespace Milou.Deployer.Web.Core.Application
{
    public static class ApplicationConstants
    {
        public const string RunAsService = "--run-as-service";

        public const string DevelopmentMode = "--development-mode";

        public const string ApplicationName = "Milou.Deployer.Web";

        public const string ApplicationTempDirectory = "urn:milou:deployer:web:temp-directory";

        public const string DotnetRunningInContainer = "DOTNET_RUNNING_IN_CONTAINER";

        public const string ProxyAddresses = "urn:milou:deployer:web:proxy-addresses";

        public const string ProxyForwardLimit = "urn:milou:deployer:web:proxy-forward-limit";
    }
}