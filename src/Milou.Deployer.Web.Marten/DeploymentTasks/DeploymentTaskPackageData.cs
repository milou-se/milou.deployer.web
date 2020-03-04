namespace Milou.Deployer.Web.Marten.DeploymentTasks
{
    [MartenData]
    public class DeploymentTaskPackageData
    {
        public string Id { get; set; }

        public string DeploymentTargetId { get; set; }

        public string[] ProcessArgs { get; set; }

        public string NuGetConfigXml { get; set; }

        public string ManifestJson { get; set; }

        public string PublishSettingsXml { get; set; }

        public string AgentId { get; set; }
    }
}