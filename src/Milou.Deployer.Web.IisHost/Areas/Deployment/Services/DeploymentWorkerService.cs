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
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Messages;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public sealed class DeploymentWorkerService : BackgroundService,
        INotificationHandler<WorkerCreated>,
        INotificationHandler<TargetEnabled>,
        INotificationHandler<TargetDisabled>,
        IRequestHandler<StartWorker>
    {
        private readonly ConfigurationInstanceHolder _configurationInstanceHolder;
        private readonly ILogger _logger;
        private readonly Dictionary<string, Task> _tasks;
        private readonly Dictionary<string, CancellationTokenSource> _cancellations;

        public DeploymentWorkerService(
            ConfigurationInstanceHolder configurationInstanceHolder,
            ILogger logger)
        {
            _configurationInstanceHolder = configurationInstanceHolder;
            _logger = logger;
            _tasks = new Dictionary<string, Task>();
            _cancellations = new Dictionary<string, CancellationTokenSource>();
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
                try
                {
                    var completedTaskKeys = _tasks
                        .Where(pair => pair.Value.IsCompletedSuccessfully)
                        .Select(pair => pair.Key)
                        .ToArray();

                    foreach (var completedTaskKey in completedTaskKeys)
                    {
                        if (_tasks.ContainsKey(completedTaskKey))
                        {
                            _tasks.Remove(completedTaskKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not clean up completed worker tasks");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            await Task.WhenAll(_tasks.Values.Where(task => !task.IsCompleted));
        }

        private void StartWorker(DeploymentTargetWorker deploymentTargetWorker, CancellationToken stoppingToken)
        {
            if (_tasks.ContainsKey(deploymentTargetWorker.TargetId))
            {
                if (deploymentTargetWorker.IsExecuting)
                {
                    return;
                }

                var task = _tasks[deploymentTargetWorker.TargetId];

                if (!task.IsCompleted)
                {
                    if (_cancellations.ContainsKey(deploymentTargetWorker.TargetId))
                    {
                        var tokenSource = _cancellations[deploymentTargetWorker.TargetId];
                        try
                        {
                            tokenSource.Cancel();
                            tokenSource.Dispose();
                        }
                        catch (ObjectDisposedException)
                        {
                        }

                        _cancellations.Remove(deploymentTargetWorker.TargetId);
                    }
                }

                _tasks.Remove(deploymentTargetWorker.TargetId);
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cancellationTokenSource.Token);

            _cancellations.TryAdd(deploymentTargetWorker.TargetId, cancellationTokenSource);

            _tasks.Add(deploymentTargetWorker.TargetId, Task.Run(() => deploymentTargetWorker.ExecuteAsync(linked.Token), linked.Token));
        }

        public Task Handle(WorkerCreated notification, CancellationToken cancellationToken)
        {
            StartWorker(notification.Worker, cancellationToken);
            return Task.CompletedTask;
        }

        public Task Handle(TargetEnabled notification, CancellationToken cancellationToken)
        {
            if (!_configurationInstanceHolder.TryGet(notification.TargetId,
                out DeploymentTargetWorker worker))
            {
                _logger.Warning("Could not get worker for target id {TargetId}", notification.TargetId);
                return Task.CompletedTask;
            }

            StartWorker(worker, cancellationToken);

            return Task.CompletedTask;
        }

        public Task<Unit> Handle(StartWorker request, CancellationToken cancellationToken)
        {
            StartWorker(request.Worker, cancellationToken);

            return Task.FromResult(Unit.Value);
        }

        public async Task Handle(TargetDisabled notification, CancellationToken cancellationToken)
        {
            if (!_configurationInstanceHolder.TryGet(notification.TargetId,
                out DeploymentTargetWorker worker))
            {
                _logger.Warning("Could not get worker for target id {TargetId}", notification.TargetId);
                return;
            }

            await StopWorkerAsync(worker, cancellationToken);
        }

        private async Task StopWorkerAsync(DeploymentTargetWorker worker, CancellationToken cancellationToken)
        {
            await worker.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (CancellationTokenSource cancellationTokenSource in _cancellations.Values)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }
    }
}
