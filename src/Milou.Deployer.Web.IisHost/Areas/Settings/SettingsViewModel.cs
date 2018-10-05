using System.Collections.Immutable;
using System.Linq;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class SettingsViewModel
    {
        public SettingsViewModel(
            string targetReadService,
            ImmutableArray<ControllerRouteInfo> routes,
            ConfigurationInfo configurationInfo,
            ImmutableArray<ContainerRegistrationInfo> containerRegistrations)
        {
            TargetReadService = targetReadService;
            ConfigurationInfo = configurationInfo;
            ContainerRegistrations = containerRegistrations.OrderBy(reg => reg.Service).ToImmutableArray();
            Routes = routes.OrderBy(route => route.Route.Value).ToImmutableArray();
        }

        public string TargetReadService { get; }

        public ConfigurationInfo ConfigurationInfo { get; }

        public ImmutableArray<ContainerRegistrationInfo> ContainerRegistrations { get; }

        public ImmutableArray<ControllerRouteInfo> Routes { get; }
    }
}