using System.Collections.Immutable;
using System.Linq;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class SettingsViewModel
    {
        public string TargetReadService { get; }
        public ConfigurationInfo ConfigurationInfo { get; }

        public ImmutableArray<ControllerRouteInfo> Routes { get; }

        public SettingsViewModel(
            string targetReadService,
            ImmutableArray<ControllerRouteInfo> routes,
            ConfigurationInfo configurationInfo)
        {
            TargetReadService = targetReadService;
            ConfigurationInfo = configurationInfo;
            Routes = routes.OrderBy(route => route.Route.Value).ToImmutableArray();
        }
    }
}