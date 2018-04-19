namespace Milou.Deployer.Web.Core.Deployment
{
    public class CreateOrganization
    {
        public CreateOrganization(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}