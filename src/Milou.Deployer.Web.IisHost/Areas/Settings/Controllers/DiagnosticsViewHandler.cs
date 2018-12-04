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
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
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

        [NotNull]
        private readonly Scope _scope;

        public DiagnosticsViewHandler(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] MultiSourceKeyValueConfiguration configuration,
            [NotNull] Scope scope,
            [NotNull] IConfiguration aspNetConfiguration,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch,
            [NotNull] EnvironmentConfiguration environmentConfiguration)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _aspNetConfiguration = aspNetConfiguration ?? throw new ArgumentNullException(nameof(aspNetConfiguration));
            _loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));
            _environmentConfiguration = environmentConfiguration;
        }

        public async Task<SettingsViewModel> Handle(SettingsViewRequest request, CancellationToken cancellationToken)
        {
            ImmutableArray<ControllerRouteInfo> routesWithController =
                RouteList.GetRoutesWithController(Assemblies.FilteredAssemblies());

            var configurationValues = new ConfigurationInfo(_configuration.SourceChain,
                _configuration.AllKeys
                    .OrderBy(key => key)
                    .Select(key =>
                        new ConfigurationKeyInfo(key,
                            _configuration[key].MakeAnonymous(key, StringExtensions.DefaultAnonymousKeyWords.ToArray()),
                            _configuration.ConfiguratorFor(key).GetType().Name))
                    .ToImmutableArray());

            ImmutableArray<ContainerRegistrationInfo> registrations = _scope.Deepest().Lifetime.ComponentRegistry
                .Registrations.SelectMany(reg => reg.Services.Select(service =>
                    new ContainerRegistrationInfo(service.Description, reg.Lifetime.ToString()))).ToImmutableArray();

            IEnumerable<KeyValuePair<string, string>> aspNetConfigurationValues = _aspNetConfiguration
                .AsEnumerable()
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair =>
                    new KeyValuePair<string, string>(pair.Key,
                        pair.Value.MakeAnonymous(pair.Key, StringExtensions.DefaultAnonymousKeyWords.ToArray())));

            ApplicationVersionInfo applicationVersionInfo = ApplicationVersionHelper.GetAppVersion();

            ImmutableArray<(object, string)> aspnetConfigurationValues = _scope.GetConfigurationValues();

            IKeyValueConfiguration applicationMetadata = await GetApplicationMetadataAsync(cancellationToken);

            var settingsViewModel = new SettingsViewModel(
                _deploymentTargetReadService.GetType().Name,
                routesWithController,
                configurationValues,
                registrations,
                aspNetConfigurationValues,
                _loggingLevelSwitch.MinimumLevel,
                applicationVersionInfo,
                aspnetConfigurationValues,
                applicationMetadata);

            return settingsViewModel;
        }

        private async Task<IKeyValueConfiguration> GetApplicationMetadataAsync(CancellationToken cancellationToken)
        {
            string applicationMetadataJsonFilePath = Path.Combine(_environmentConfiguration.ContentBasePath,
                "wwwroot",
                "applicationmetadata.json");

            if (!File.Exists(applicationMetadataJsonFilePath))
            {
                return NoConfiguration.Empty;
            }

            string json = await File.ReadAllTextAsync(applicationMetadataJsonFilePath, Encoding.UTF8, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return NoConfiguration.Empty;
            }

            ConfigurationItems configurationItems = new JsonConfigurationSerializer().Deserialize(json);

            if (configurationItems is null)
            {
                return NoConfiguration.Empty;
            }

            if (configurationItems.Keys.IsDefaultOrEmpty)
            {
                return NoConfiguration.Empty;
            }

            var values = new NameValueCollection();

            foreach (KeyValue configurationItem in configurationItems.Keys)
            {
                values.Add(configurationItem.Key, configurationItem.Value);
            }

            return new InMemoryKeyValueConfiguration(values);
        }
    }
}