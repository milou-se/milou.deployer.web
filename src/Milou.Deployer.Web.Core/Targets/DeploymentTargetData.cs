using System.Collections.Generic;
using Marten.Schema;

namespace Milou.Deployer.Web.Core.Targets
{
    public class DeploymentTargetData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool AllowExplicitPreRelease { get; set; }

        public ICollection<string> AllowedPackageNames { get; set; } = new HashSet<string>();

        [ForeignKey(typeof(ProjectData))]
        public string ProjectId { get; set; }
    }
}