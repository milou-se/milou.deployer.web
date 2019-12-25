using System.Reflection;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Metadata;

namespace Arbor.App.Extensions.Application
{
    public static class ApplicationConstants
    {
        public const string AssemblyPrefix = "urn:arbor:app:web:assembly-prefix";

        public const string AspNetEnvironment = "ASPNETCORE_ENVIRONMENT";

        public const string EnvironmentProduction = "Production";

        public const string RunAsService = "--run-as-service";

        public const string DevelopmentMode = "--development-mode";

        public const string ApplicationNameKey = "urn:arbor:app:name";

        public const string UseExplicitPorts = "urn:arbor:app:web:use-explicit-ports";

        public const string PublicHostName = "urn:arbor:app:web:public-hostname";

        public const string PublicPort = "urn:arbor:app:web:public-port";

        public const string PublicPortIsHttps = "urn:arbor:app:web:public-port-is-https";

        public const string HttpPort = "urn:arbor:app:web:http-port";

        public const string HttpsPort = "urn:arbor:app:web:https-port";


        public const string ApplicationTempDirectory = "urn:arbor:app:web:temp-directory";

        public const string DotnetRunningInContainer = "DOTNET_RUNNING_IN_CONTAINER";

        public const string ProxyAddresses = "urn:arbor:app:web:proxy-addresses";

        [Metadata(defaultValue: "0")]
        public const string ProxyForwardLimit = "urn:arbor:app:web:proxy-forward-limit";

        public const string PfxFile = "urn:arbor:app:web:pfx-file";

        public const string PfxPassword = "urn:arbor:app:web:pfx-password";
    }

    public static class ApplicationNameHelper
    {
        public static string GetApplicationName(this IKeyValueConfiguration keyValueConfiguration)
        {
            string name = keyValueConfiguration[ApplicationConstants.ApplicationNameKey];

            if (string.IsNullOrWhiteSpace(name))
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }

            return name;
        }
    }
}