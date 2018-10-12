using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Application
{
    public class EnvironmentConfiguration
    {
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
    }
}