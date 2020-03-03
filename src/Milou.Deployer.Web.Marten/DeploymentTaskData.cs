namespace Milou.Deployer.Web.Marten
{
    [MartenData]
    public class DeploymentTaskData
    {
        public string Id { get; set; }

        public string PackageVersion { get; set; }

        public string DeploymentTargetId { get; set; }

        public string StartedBy { get; set; }

        public string AgentId { get; set; }
    }
}