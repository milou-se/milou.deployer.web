namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    public class DeploymentLogViewOutputModel
    {
        public string Log { get; }

        public DeploymentLogViewOutputModel(string log)
        {
            Log = log;
        }
    }
}