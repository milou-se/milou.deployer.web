using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.Core.Application
{
    public static class ApplicationConstants
    {
        public const string AspNetEnvironment = "ASPNETCORE_ENVIRONMENT";

        public const string EnvironmentProduction = "Production";

        public const string ApplicationName = "Milou.Deployer.Web";

        public const string RunAsService = "--run-as-service";

        public const string DevelopmentMode = "--development-mode";

        public const string UseExplicitPorts = "urn:milou:deployer:web:use-explicit-ports";

        public const string PublicHostName = "urn:milou:deployer:web:public-hostname";

        public const string PublicPort = "urn:milou:deployer:web:public-port";

        public const string PublicPortIsHttps = "urn:milou:deployer:web:public-port-is-https";

        public const string HttpPort = "urn:milou:deployer:web:http-port";

        public const string HttpsPort = "urn:milou:deployer:web:https-port";


        public const string ApplicationTempDirectory = "urn:milou:deployer:web:temp-directory";

        public const string DotnetRunningInContainer = "DOTNET_RUNNING_IN_CONTAINER";

        public const string ProxyAddresses = "urn:milou:deployer:web:proxy-addresses";

        [Metadata(defaultValue: "0")]
        public const string ProxyForwardLimit = "urn:milou:deployer:web:proxy-forward-limit";

        public const string PfxFile = "urn:milou:deployer:web:pfx-file";

        public const string PfxPassword = "urn:milou:deployer:web:pfx-password";
    }
}