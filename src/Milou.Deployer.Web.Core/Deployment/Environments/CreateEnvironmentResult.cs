namespace Milou.Deployer.Web.Core.Deployment.Environments
{
    public class CreateEnvironmentResult
    {
        public CreateEnvironmentResult(string id, Result status)
        {
            Id = id;
            Status = status;
        }

        public string Id { get; }

        public Result Status { get; }
    }
}