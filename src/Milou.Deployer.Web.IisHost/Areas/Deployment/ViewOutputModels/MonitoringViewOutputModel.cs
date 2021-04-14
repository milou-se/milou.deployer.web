using System.Collections.Generic;
using Arbor.App.Extensions;
using Arbor.App.Extensions.ExtensionMethods;
using Milou.Deployer.Web.Core.Application.Metadata;

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
