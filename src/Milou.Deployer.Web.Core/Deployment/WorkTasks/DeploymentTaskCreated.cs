namespace Milou.Deployer.Web.Core.Deployment.WorkTasks
{
    public class DeploymentTaskCreated
    {
        public DeploymentTaskCreated(string deploymentTaskId, string packageVersion, string deploymentTargetId, string startedBy)
        {
            DeploymentTaskId = deploymentTaskId;
            PackageVersion = packageVersion;
            DeploymentTargetId = deploymentTargetId;
            StartedBy = startedBy;
        }

        public string DeploymentTaskId { get; }

        public string PackageVersion { get; }

        public string DeploymentTargetId { get; }

        public string StartedBy { get; }
    }
}