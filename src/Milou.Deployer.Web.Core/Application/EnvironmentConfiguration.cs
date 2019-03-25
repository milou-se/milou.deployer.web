using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using JetBrains.Annotations;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Core.Application
{
    public class EnvironmentConfiguration : IConfigurationValues
    {
        public EnvironmentConfiguration()
        {
            HttpPort = 34343;
            UseExplicitPorts = true;
        }

        public bool UseVerboseExceptions => !EnvironmentName.HasValue()
                                            || EnvironmentName.Equals(ApplicationConstants.EnvironmentProduction,
                                                StringComparison.OrdinalIgnoreCase);

        [PublicAPI]
        public string ApplicationBasePath { get; set; }

        [PublicAPI]
        public string ContentBasePath { get; set; }

        [PublicAPI]
        public string EnvironmentName { get; set; }

        [PublicAPI]
        public string PublicHostname { get; set; }

        public bool UseExplicitPorts { get; set; }

        [PublicAPI]
        public int? PublicPort { get; set; }

        [PublicAPI]
        public bool PublicPortIsHttps { get; set; }

        [PublicAPI]
        public int? HttpPort { get; set; }

        [PublicAPI]
        public int? HttpsPort { get; set; }

        [PublicAPI]
        public string PfxFile { get; set; }

        [PublicAPI]
        public string PfxPassword { get; set; }

        public bool UseVerboseLogging { get; set; }

        public bool IsDevelopmentMode { get; set; }

        public int? ForwardLimit { get; set; }

        public ImmutableArray<string> CommandLineArgs { get; set; }

        public List<IPAddress> ProxyAddresses { get; } = new List<IPAddress>();

        public override string ToString()
        {
            return
                $"{nameof(ApplicationBasePath)}: {ApplicationBasePath}, {nameof(ContentBasePath)}: {ContentBasePath}, {nameof(EnvironmentName)}: {EnvironmentName}, {nameof(HttpPort)}: {HttpPort}, {nameof(HttpsPort)}: {HttpsPort}, {nameof(PfxFile)}: {PfxFile}, {nameof(PfxPassword)}: *****";
        }
    }
}
