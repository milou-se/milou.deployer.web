using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;
using Milou.Deployer.Web.IisHost.AspNetCore;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class SettingsViewModel
    {
        public SettingsViewModel(
            string targetReadService,
            ImmutableArray<ControllerRouteInfo> routes,
            ConfigurationInfo configurationInfo,
            ImmutableArray<ServiceRegistrationInfo> serviceRegistrations,
            IEnumerable<KeyValuePair<string, string>> aspNetConfigurationValues,
            IEnumerable<ServiceInstance> registrationInstances,
            LogEventLevel logEventLevel,
            ApplicationVersionInfo applicationVersionInfo,
            IKeyValueConfiguration applicationmetadata)
        {
            AspNetConfigurationValues = aspNetConfigurationValues.OrderBy(x => x.Key).ToImmutableArray();
            TargetReadService = targetReadService;
            ConfigurationInfo = configurationInfo;
            RegistrationInstances = registrationInstances.OrderBy(serviceInstance => serviceInstance.RegistrationType.FullName).ToImmutableArray();
            ServiceRegistrations = serviceRegistrations.OrderBy(serviceRegistrationInfo => serviceRegistrationInfo.ServiceDescriptorServiceType.FullName)
                .ToImmutableArray();
            LogEventLevel = logEventLevel;
            ApplicationVersionInfo = applicationVersionInfo;
            Applicationmetadata = applicationmetadata;
            Routes = routes.OrderBy(route => route.Route.Value).ToImmutableArray();
            ConfigurationValues = ImmutableArray<(object, string)>.Empty;
        }

        public ImmutableArray<KeyValuePair<string, string>> AspNetConfigurationValues { get; }

        public string TargetReadService { get; }

        public ConfigurationInfo ConfigurationInfo { get; }

        public ImmutableArray<ServiceInstance> RegistrationInstances { get; }

        public ImmutableArray<ServiceRegistrationInfo> ServiceRegistrations { get; }

        public LogEventLevel LogEventLevel { get; }

        public ApplicationVersionInfo ApplicationVersionInfo { get; }

        public ImmutableArray<(object, string)> ConfigurationValues { get; }

        public IKeyValueConfiguration Applicationmetadata { get; }

        public ImmutableArray<ControllerRouteInfo> Routes { get; }
    }
}
