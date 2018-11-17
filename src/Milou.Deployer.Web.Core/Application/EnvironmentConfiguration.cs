using System;
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
        }

        public bool UseVerboseExceptions => !EnvironmentName.HasValue()
                                            || EnvironmentName.Equals("production",
                                                StringComparison.OrdinalIgnoreCase);

        [PublicAPI]
        public string ApplicationBasePath { get; set; }

        [PublicAPI]
        public string ContentBasePath { get; set; }

        [PublicAPI]
        public string EnvironmentName { get; set; }

        [PublicAPI]
        public int? HttpPort { get; set; }

        [PublicAPI]
        public int? HttpsPort { get; set; }

        [PublicAPI]
        public string PfxFile { get; set; }

        [PublicAPI]
        public string PfxPassword { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(ApplicationBasePath)}: {ApplicationBasePath}, {nameof(ContentBasePath)}: {ContentBasePath}, {nameof(EnvironmentName)}: {EnvironmentName}, {nameof(HttpPort)}: {HttpPort}, {nameof(HttpsPort)}: {HttpsPort}, {nameof(PfxFile)}: {PfxFile}, {nameof(PfxPassword)}: *****";
        }
    }
}