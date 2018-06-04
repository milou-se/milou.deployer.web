namespace Milou.Deployer.Web.Core.Application
{
    public class EnvironmentConfiguration
    {
        public string ApplicationBasePath { get; set; }

        public string ApplicationViewBasePath { get; set; }

        public string EnvironmentName { get; set; }

        public int? HttpPort { get; set; }

        public int? HttpsPort { get; set; }

        public string PfxFile { get; set; }

        public string PfxPassword { get; set; }

        public string HostName { get; set; }
    }
}
