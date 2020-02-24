using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Time;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Startup;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class WorkerSetupStartupTask : BackgroundService, IStartupTask
    {
        private readonly IKeyValueConfiguration _configuration;
        private readonly IServiceProvider _deploymentService;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly ConfigurationInstanceHolder _holder;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly WorkerConfiguration _workerConfiguration;
        private readonly TimeoutHelper _timeoutHelper;

        private readonly ICustomClock _clock;
        private readonly IServiceProvider _serviceProvider;

        public WorkerSetupStartupTask(
            IKeyValueConfiguration configuration,
            ILogger logger,
            IDeploymentTargetReadService deploymentTargetReadService,
            ConfigurationInstanceHolder holder,
            IServiceProvider deploymentService,
            IMediator mediator,
            WorkerConfiguration workerConfiguration,
            TimeoutHelper timeoutHelper,
            ICustomClock clock,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _deploymentTargetReadService = deploymentTargetReadService;
            _holder = holder;
            _deploymentService = deploymentService;
            _mediator = mediator;
            _workerConfiguration = workerConfiguration;
            _timeoutHelper = timeoutHelper;
            _clock = clock;
            _serviceProvider = serviceProvider;
        }

        public bool IsCompleted { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            IReadOnlyCollection<string> targetIds;

            try
            {
                if (!int.TryParse(
                        _configuration[DeployerAppConstants.StartupTargetsTimeoutInSeconds],
                        out int startupTimeoutInSeconds) ||
                    startupTimeoutInSeconds <= 0)
                {
                    startupTimeoutInSeconds = 30;
                }

                using (var startupToken = _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(startupTimeoutInSeconds)))
                {
                    using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken,
                        startupToken.Token))
                    {
                        targetIds =
                            (await _deploymentTargetReadService.GetDeploymentTargetsAsync(stoppingToken: linkedToken.Token))
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

            foreach (string targetId in targetIds)
            {
                var deploymentTargetWorker = new DeploymentTargetWorker(targetId, _logger, _mediator, _workerConfiguration, _timeoutHelper, _clock, _serviceProvider);

                _holder.Add(new NamedInstance<DeploymentTargetWorker>(
                    deploymentTargetWorker,
                    targetId));

                await _mediator.Send(new StartWorker(deploymentTargetWorker), stoppingToken);
            }

            IsCompleted = true;
        }
    }
}
