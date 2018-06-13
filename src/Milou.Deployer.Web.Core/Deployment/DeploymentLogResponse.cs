namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentLogResponse
    {
        public DeploymentLogResponse(string log)
        {
            Log = log;
        }

        public string Log { get; }
    }
}