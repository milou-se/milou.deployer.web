using Marten.Schema;

namespace Milou.Deployer.Web.Marten
{
    [MartenData]
    public class ProjectData
    {
        [ForeignKey(typeof(OrganizationData))]
        public string OrganizationId { get; set; }

        public string Id { get; set; }
    }
}
