namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewInputModels
{
    public class DeploymentTaskInput
    {
        public string TargetId { get; set; }

        public string PackageVersion { get; set; }
    }
}