namespace Milou.Deployer.Web.Core.Deployment
{
    public class CreateProject
    {
        public string Id { get; }
        public string OrganizationId { get; }

        public CreateProject(string id, string organizationId)
        {
            Id = id;
            OrganizationId = organizationId;
        }
    }
}