using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Messages;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class DeploymentWorkerService : BackgroundService, INotificationHandler<WorkerCreated>, IRequestHandler<StartWorker>
    {
        private readonly ConfigurationInstanceHolder _configurationInstanceHolder;
        private readonly ILogger _logger;
        private readonly List<Task> _tasks;

        public DeploymentWorkerService(
            ConfigurationInstanceHolder configurationInstanceHolder,
            ILogger logger)
        {
            _configurationInstanceHolder = configurationInstanceHolder;
            _logger = logger;
            _tasks = new List<Task>();
        }

        private DeploymentTargetWorker GetWorkerByTargetId([NotNull] string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            if (!_configurationInstanceHolder.TryGet(targetId,
                out DeploymentTargetWorker worker))
            {
                _logger.Warning("Could not get worker for target id {TargetId}", targetId);
            }

            return worker;
        }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            var foundWorker = GetWorkerByTargetId(deploymentTask.DeploymentTargetId);

            if (foundWorker is null)
            {
                _logger.Error("Could not find worker for deployment target id {DeploymentTargetId}",
                    deploymentTask.DeploymentTargetId);
                return;
            }

            foundWorker.Enqueue(deploymentTask);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deploymentTargetWorkers = _configurationInstanceHolder.GetInstances<DeploymentTargetWorker>().Values
                .ToImmutableArray();

            foreach (var deploymentTargetWorker in deploymentTargetWorkers)
            {
                StartWorker(deploymentTargetWorker, stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            await Task.WhenAll(_tasks.Where(task => !task.IsCompleted));
        }

        private void StartWorker(DeploymentTargetWorker deploymentTargetWorker, CancellationToken stoppingToken)
        {
            _tasks.Add(Task.Run(() => deploymentTargetWorker.ExecuteAsync(stoppingToken), stoppingToken));
        }

        public Task Handle(WorkerCreated notification, CancellationToken cancellationToken)
        {
            StartWorker(notification.Worker, cancellationToken);
            return Task.CompletedTask;
        }

        public Task<Unit> Handle(StartWorker request, CancellationToken cancellationToken)
        {
            StartWorker(request.Worker, cancellationToken);

            return Task.FromResult(Unit.Value);
        }
    }
}
