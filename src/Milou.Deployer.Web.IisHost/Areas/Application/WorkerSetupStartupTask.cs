using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Startup;
using Milou.Deployer.Web.Core.Time;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class WorkerSetupStartupTask : BackgroundService, IStartupTask
    {
        private readonly IKeyValueConfiguration _configuration;
        private readonly DeploymentService _deploymentService;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly ConfigurationInstanceHolder _holder;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly WorkerConfiguration _workerConfiguration;
        private readonly TimeoutHelper _timeoutHelper;

        public WorkerSetupStartupTask(
            IKeyValueConfiguration configuration,
            ILogger logger,
            IDeploymentTargetReadService deploymentTargetReadService,
            ConfigurationInstanceHolder holder,
            DeploymentService deploymentService,
            IMediator mediator,
            WorkerConfiguration workerConfiguration,
            TimeoutHelper timeoutHelper)
        {
            _configuration = configuration;
            _logger = logger;
            _deploymentTargetReadService = deploymentTargetReadService;
            _holder = holder;
            _deploymentService = deploymentService;
            _mediator = mediator;
            _workerConfiguration = workerConfiguration;
            _timeoutHelper = timeoutHelper;
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

                using (var startupToken = _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(startupTimeoutInSeconds)))
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

            foreach (var targetId in targetIds)
            {
                var deploymentTargetWorker = new DeploymentTargetWorker(targetId, _deploymentService, _logger, _mediator, _workerConfiguration, _timeoutHelper);

                _holder.Add(new NamedInstance<DeploymentTargetWorker>(
                    deploymentTargetWorker,
                    targetId));

                await _mediator.Send(new StartWorker(deploymentTargetWorker), stoppingToken);
            }

            IsCompleted = true;
        }
    }
}
