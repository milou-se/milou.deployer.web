using System.Collections.Generic;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    public class DeploymentLogViewOutputModel
    {
        public IReadOnlyCollection<LogItem> LogItems { get; }

        public DeploymentLogViewOutputModel(IReadOnlyCollection<LogItem> logItems)
        {
            LogItems = logItems;
        }
    }
}
