namespace Milou.Deployer.Web.Marten.AutoDeploy
{
    [MartenData]
    public class AutoDeployData
    {
        public bool Enabled { get; set; }

        public bool PollingEnabled { get; set; }
    }
}