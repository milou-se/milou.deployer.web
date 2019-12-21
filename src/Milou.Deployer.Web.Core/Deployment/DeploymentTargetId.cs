namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentTargetId
    {
        public string TargetId { get; }

        public DeploymentTargetId(string targetId)
        {
            TargetId = targetId;
        }
    }
}