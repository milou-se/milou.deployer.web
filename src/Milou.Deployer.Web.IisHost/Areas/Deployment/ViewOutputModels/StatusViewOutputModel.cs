namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class StatusViewOutputModel
    {
        public StatusViewOutputModel(string deploymentTargetId)
        {
            DeploymentTargetId = deploymentTargetId;
        }

        public string DeploymentTargetId { get; }
    }
}
