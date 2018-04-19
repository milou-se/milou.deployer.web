namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class StatusViewOutputModel
    {
        public string DeploymentTargetId { get; }

        public StatusViewOutputModel(string deploymentTargetId)
        {
            DeploymentTargetId = deploymentTargetId;
        }
    }
}