using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Schema.Json;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Application.Metadata;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.AspNetCore.Hosting;
using Serilog.Core;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    [UsedImplicitly]
    public class DiagnosticsViewHandler : IRequestHandler<SettingsViewRequest, SettingsViewModel>
    {
        private readonly IConfiguration _aspNetConfiguration;
        private readonly MultiSourceKeyValueConfiguration _configuration;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;

        [NotNull]
        private readonly EnvironmentConfiguration _environmentConfiguration;

        private readonly LoggingLevelSwitch _loggingLevelSwitch;
        private readonly ServiceDiagnostics _serviceDiagnostics;

        private readonly IServiceProvider _serviceProvider;
        private readonly ConfigurationInstanceHolder _configurationInstanceHolder;

        public DiagnosticsViewHandler(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] MultiSourceKeyValueConfiguration configuration,
            [NotNull] IConfiguration aspNetConfiguration,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch,
            [NotNull] EnvironmentConfiguration environmentConfiguration,
            IServiceProvider serviceProvider,
            ServiceDiagnostics serviceDiagnostics,
            ConfigurationInstanceHolder configurationInstanceHolder)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _aspNetConfiguration = aspNetConfiguration ?? throw new ArgumentNullException(nameof(aspNetConfiguration));
            _loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));
            _environmentConfiguration = environmentConfiguration;
            _serviceProvider = serviceProvider;
            _serviceDiagnostics = serviceDiagnostics;
            _configurationInstanceHolder = configurationInstanceHolder;
        }

        private async Task<IKeyValueConfiguration> GetApplicationMetadataAsync(CancellationToken cancellationToken)
        {
            var applicationMetadataJsonFilePath = Path.Combine(_environmentConfiguration.ContentBasePath,
                "wwwroot",
                "applicationmetadata.json");

            if (!File.Exists(applicationMetadataJsonFilePath))
            {
                return NoConfiguration.Empty;
            }

            var json = await File.ReadAllTextAsync(applicationMetadataJsonFilePath, Encoding.UTF8, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return NoConfiguration.Empty;
            }

            var configurationItems = JsonConfigurationSerializer.Deserialize(json);

            if (configurationItems is null)
            {
                return NoConfiguration.Empty;
            }

            if (configurationItems.Keys.IsDefaultOrEmpty)
            {
                return NoConfiguration.Empty;
            }

            var values = new NameValueCollection();

            foreach (var configurationItem in configurationItems.Keys)
            {
                values.Add(configurationItem.Key, configurationItem.Value);
            }

            return new InMemoryKeyValueConfiguration(values);
        }

        public async Task<SettingsViewModel> Handle(SettingsViewRequest request, CancellationToken cancellationToken)
        {
            var routesWithController =
                RouteList.GetRoutesWithController(ApplicationAssemblies.FilteredAssemblies());

            var configurationValues = new ConfigurationInfo(_configuration.SourceChain,
                _configuration.AllKeys
                    .OrderBy(key => key)
                    .Select(key =>
                        new ConfigurationKeyInfo(key,
                            _configuration[key].MakeAnonymous(key, ApplicationStringExtensions.DefaultAnonymousKeyWords.ToArray()),
                            _configuration.ConfiguratorFor(key).GetType().Name))
                    .ToImmutableArray());


            var aspNetConfigurationValues = _aspNetConfiguration
                .AsEnumerable()
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair =>
                    new KeyValuePair<string, string>(pair.Key,
                        pair.Value.MakeAnonymous(pair.Key, ApplicationStringExtensions.DefaultAnonymousKeyWords.ToArray())));

            var applicationVersionInfo = ApplicationVersionHelper.GetAppVersion();

            var serviceDiagnosticsRegistrations = _serviceDiagnostics.Registrations;

            var applicationMetadata = await GetApplicationMetadataAsync(cancellationToken);

            ServiceInstance GetInstance(ServiceRegistrationInfo serviceRegistrationInfo)
            {
                var registrationType = serviceRegistrationInfo.ServiceDescriptorServiceType;

                if (serviceRegistrationInfo.ServiceDescriptorImplementationInstance != null)
                {
                    return new ServiceInstance(registrationType,
                        serviceRegistrationInfo.ServiceDescriptorImplementationInstance,
                        serviceRegistrationInfo.Module);
                }

                if (serviceRegistrationInfo.Factory != null)
                {
                    try
                    {
                        return new ServiceInstance(
                            registrationType,
                            serviceRegistrationInfo.Factory(_serviceProvider),
                            serviceRegistrationInfo.Module);
                    }
                    catch (Exception ex)
                    {
                        return new ServiceInstance(registrationType, ex, serviceRegistrationInfo.Module);
                    }
                }

                if (serviceRegistrationInfo.ServiceDescriptorImplementationType.IsGenericType)
                {
                    return new ServiceInstance(registrationType, "Generic type", serviceRegistrationInfo.Module);
                }

                return new ServiceInstance(registrationType,
                    _serviceProvider.GetService(serviceRegistrationInfo.ServiceDescriptorImplementationType),
                    serviceRegistrationInfo.Module);
            }

            var deploymentTargetWorkers = _configurationInstanceHolder.GetInstances<DeploymentTargetWorker>().Values
                .ToImmutableArray();

            var settingsViewModel = new SettingsViewModel(
                _deploymentTargetReadService.GetType().Name,
                routesWithController,
                configurationValues,
                serviceDiagnosticsRegistrations,
                aspNetConfigurationValues,
                serviceDiagnosticsRegistrations
                    .Select(GetInstance).ToImmutableArray(),
                _loggingLevelSwitch.MinimumLevel,
                applicationVersionInfo,
                applicationMetadata,
                deploymentTargetWorkers);

            return settingsViewModel;
        }
    }
}
