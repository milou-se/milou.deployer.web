namespace Milou.Deployer.Web.Marten.Settings
{
    [MartenData]
    public class NexusConfigData
    {
        public string HmacKey { get; set; }

        public string NuGetConfig { get; set; }

        public string NuGetSource { get; set; }
    }
}