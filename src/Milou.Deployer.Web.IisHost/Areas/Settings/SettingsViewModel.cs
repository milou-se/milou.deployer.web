using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class SettingsViewModel
    {
        public SettingsViewModel(
            string targetReadService,
            ImmutableArray<ControllerRouteInfo> routes,
            ConfigurationInfo configurationInfo,
            ImmutableArray<ContainerRegistrationInfo> containerRegistrations,
            IEnumerable<KeyValuePair<string, string>> aspNetConfigurationValues,
            LogEventLevel logEventLevel,
            ApplicationVersionInfo applicationVersionInfo,
            ImmutableArray<(object, string)> configurationValues,
            IKeyValueConfiguration applicationmetadata)
        {
            AspNetConfigurationValues = aspNetConfigurationValues.OrderBy(x => x.Key).ToImmutableArray();
            TargetReadService = targetReadService;
            ConfigurationInfo = configurationInfo;
            LogEventLevel = logEventLevel;
            ApplicationVersionInfo = applicationVersionInfo;
            ConfigurationValues = configurationValues;
            Applicationmetadata = applicationmetadata;
            ContainerRegistrations = containerRegistrations.OrderBy(reg => reg.Service).ToImmutableArray();
            Routes = routes.OrderBy(route => route.Route.Value).ToImmutableArray();
        }

        public ImmutableArray<KeyValuePair<string, string>> AspNetConfigurationValues { get; }

        public string TargetReadService { get; }

        public ConfigurationInfo ConfigurationInfo { get; }

        public LogEventLevel LogEventLevel { get; }

        public ApplicationVersionInfo ApplicationVersionInfo { get; }

        public ImmutableArray<(object, string)> ConfigurationValues { get; }

        public IKeyValueConfiguration Applicationmetadata { get; }

        public ImmutableArray<ContainerRegistrationInfo> ContainerRegistrations { get; }

        public ImmutableArray<ControllerRouteInfo> Routes { get; }
    }
}