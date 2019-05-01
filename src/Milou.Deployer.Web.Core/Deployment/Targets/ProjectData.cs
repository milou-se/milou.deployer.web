using Marten.Schema;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class ProjectData
    {
        [ForeignKey(typeof(OrganizationData))]
        public string OrganizationId { get; set; }

        public string Id { get; set; }
    }
}
