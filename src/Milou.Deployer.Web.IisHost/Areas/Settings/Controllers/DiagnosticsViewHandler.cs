using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    [UsedImplicitly]
    public class DiagnosticsViewHandler : IRequestHandler<SettingsViewRequest, SettingsViewModel>
    {
        private readonly MultiSourceKeyValueConfiguration _configuration;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;

        public DiagnosticsViewHandler(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] MultiSourceKeyValueConfiguration configuration)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

            var settingsViewModel = new SettingsViewModel(
                _deploymentTargetReadService.GetType().Name,
                routesWithController,
                info);

            return Task.FromResult(settingsViewModel);
        }
    }
}