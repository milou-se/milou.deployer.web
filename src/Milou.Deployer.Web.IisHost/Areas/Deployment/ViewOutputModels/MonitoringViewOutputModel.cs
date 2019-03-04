using System.Collections.Generic;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class MonitoringViewOutputModel
    {
        public MonitoringViewOutputModel(IEnumerable<AppVersion> appVersions)
        {
            AppVersions = appVersions.SafeToReadOnlyCollection();
        }

        public IReadOnlyCollection<AppVersion> AppVersions { get; }
    }
}
