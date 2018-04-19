using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class DeploymentTargetWorker : BackgroundService
    {
        private static readonly BlockingCollection<DeploymentTask> _Queue = new BlockingCollection<DeploymentTask>();
        private readonly DeploymentService _deploymentService;
        private readonly ILogger _logger;

        public DeploymentTargetWorker(
            [NotNull] string targetId,
            [NotNull] DeploymentService deploymentService,
            [NotNull] ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            TargetId = targetId;
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string TargetId { get; }

        public DeploymentTask CurrentTask { get; private set; }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            deploymentTask.Status = WorkTaskStatus.Enqueued;
            _Queue.Add(deploymentTask);
            _logger.Information("Enqueued deployment task {DeploymentTask}", deploymentTask);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DeploymentTask deploymentTask = _Queue.Take();

                _logger.Information("Deployment target worker has taken {DeploymentTask}", deploymentTask);

                deploymentTask.Status = WorkTaskStatus.Started;

                CurrentTask = deploymentTask;

                _logger.Information("Executing deployment task {DeploymentTask}", deploymentTask);

                await _deploymentService.ExecuteDeploymentAsync(deploymentTask, _logger, stoppingToken);

                _logger.Information("Executed deployment task {DeploymentTask}", deploymentTask);

                deploymentTask.Status = WorkTaskStatus.Done;

                deploymentTask.Log("{\"Message\": \"Work task completed\"}");

                CurrentTask = null;
            }

            _Queue?.Dispose();
        }
    }
}