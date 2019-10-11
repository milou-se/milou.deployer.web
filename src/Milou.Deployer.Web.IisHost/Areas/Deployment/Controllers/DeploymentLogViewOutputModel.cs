using System;
using System.Collections.Generic;
using System.Linq;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Newtonsoft.Json;

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
