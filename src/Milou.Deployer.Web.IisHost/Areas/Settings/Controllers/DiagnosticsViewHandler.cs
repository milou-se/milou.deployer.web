using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Serilog.Core;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    [UsedImplicitly]
    public class DiagnosticsViewHandler : IRequestHandler<SettingsViewRequest, SettingsViewModel>
    {
        private readonly IConfiguration _aspNetConfiguration;
        private readonly MultiSourceKeyValueConfiguration _configuration;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly LoggingLevelSwitch _loggingLevelSwitch;

        [NotNull]
        private readonly Scope _scope;

        public DiagnosticsViewHandler(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] MultiSourceKeyValueConfiguration configuration,
            [NotNull] Scope scope,
            [NotNull] IConfiguration aspNetConfiguration,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _aspNetConfiguration = aspNetConfiguration ?? throw new ArgumentNullException(nameof(aspNetConfiguration));
            _loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));
        }

        public Task<SettingsViewModel> Handle(SettingsViewRequest request, CancellationToken cancellationToken)
        {
            ImmutableArray<ControllerRouteInfo> routesWithController =
                RouteList.GetRoutesWithController(Assemblies.FilteredAssemblies());

            var info = new ConfigurationInfo(_configuration.SourceChain,
                _configuration.AllKeys
                    .OrderBy(item => item)
                    .Select(item =>
                        new ConfigurationKeyInfo(item,
                            _configuration[item],
                            _configuration.ConfiguratorFor(item).GetType().Name))
                    .ToImmutableArray());

            ImmutableArray<ContainerRegistrationInfo> registrations = _scope.Deepest().Lifetime.ComponentRegistry
                .Registrations.SelectMany(reg => reg.Services.Select(service =>
                    new ContainerRegistrationInfo(service.Description, reg.Lifetime.ToString()))).ToImmutableArray();

            IEnumerable<KeyValuePair<string, string>> aspNetConfigurationValues = _aspNetConfiguration
                .AsEnumerable()
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair =>
                    new KeyValuePair<string, string>(pair.Key, pair.Value.MakeAnonymous(pair.Key, StringExtensions.DefaultAnonymousKeyWords.ToArray())));

            var settingsViewModel = new SettingsViewModel(
                _deploymentTargetReadService.GetType().Name,
                routesWithController,
                info,
                registrations,
                aspNetConfigurationValues,
                _loggingLevelSwitch.MinimumLevel);

            return Task.FromResult(settingsViewModel);
        }
    }
}