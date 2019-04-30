using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class WorkerSetupStartupTask : BackgroundService, IStartupTask
    {
        private readonly IKeyValueConfiguration _configuration;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly ILogger _logger;

        public WorkerSetupStartupTask(
            IKeyValueConfiguration configuration,
            ILogger logger,
            IDeploymentTargetReadService deploymentTargetReadService)
        {
            _configuration = configuration;
            _logger = logger;
            _deploymentTargetReadService = deploymentTargetReadService;
        }

        public bool IsCompleted { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IReadOnlyCollection<string> targetIds;

            try
            {
                if (!int.TryParse(_configuration[ConfigurationConstants.StartupTargetsTimeoutInSeconds],
                        out var startupTimeoutInSeconds) ||
                    startupTimeoutInSeconds <= 0)
                {
                    startupTimeoutInSeconds = 10;
                }

                using (var startupToken = new CancellationTokenSource(TimeSpan.FromSeconds(startupTimeoutInSeconds)))
                {
                    using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken,
                        startupToken.Token))
                    {
                        targetIds =
                            (await _deploymentTargetReadService.GetDeploymentTargetsAsync(linkedToken.Token))
                            .Select(deploymentTarget => deploymentTarget.Id)
                            .ToArray();

                        _logger.Debug("Found deployment target IDs {IDs}", targetIds);
                    }
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Warning(ex, "Could not get target ids");
                IsCompleted = true;
                return;
            }

            foreach (var VARIABLE in targetIds)
            {
            }

            IsCompleted = true;
        }
    }
}
