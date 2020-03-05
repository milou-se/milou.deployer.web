using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions.Tasks;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Agents;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Messages;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Signaling;
using Milou.Deployer.Web.Marten;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public sealed class DeploymentWorkerService : BackgroundService,
        INotificationHandler<WorkerCreated>,
        INotificationHandler<TargetEnabled>,
        INotificationHandler<TargetDisabled>,
        INotificationHandler<AgentLogNotification>,
        IRequestHandler<StartWorker>,
        INotificationHandler<AgentDeploymentDoneNotification>,
        INotificationHandler<AgentDeploymentFailedNotification>
    {
        private readonly Dictionary<string, CancellationTokenSource> _cancellations;
        private readonly ConfigurationInstanceHolder _configurationInstanceHolder;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly Dictionary<string, Task> _tasks;

        public DeploymentWorkerService(
            ConfigurationInstanceHolder configurationInstanceHolder,
            ILogger logger,
            IMediator mediator)
        {
            _configurationInstanceHolder = configurationInstanceHolder;
            _logger = logger;
            _mediator = mediator;
            _tasks = new Dictionary<string, Task>();
            _cancellations = new Dictionary<string, CancellationTokenSource>();
        }

        public Task Handle(AgentDeploymentDoneNotification notification, CancellationToken cancellationToken)
        {
            var workerByTargetId = GetWorkerByTargetId(notification.DeploymentTargetId);

            workerByTargetId?.NotifyDeploymentDone(notification);

            return Task.CompletedTask;
        }

        public Task Handle(AgentDeploymentFailedNotification notification, CancellationToken cancellationToken)
        {
            var workerByTargetId = GetWorkerByTargetId(notification.DeploymentTargetId);

            workerByTargetId?.NotifyDeploymentFailed(notification);

            return Task.CompletedTask;
        }

        public Task Handle(AgentLogNotification notification, CancellationToken cancellationToken)
        {
            var workerByTargetId = GetWorkerByTargetId(notification.DeploymentTargetId);

            workerByTargetId?.LogProgress(notification);

            return Task.CompletedTask;
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

        public Task Handle(WorkerCreated notification, CancellationToken cancellationToken)
        {
            if (!_configurationInstanceHolder.TryGet(
                notification.Worker.TargetId,
                out DeploymentTargetWorker _))
            {
                _configurationInstanceHolder.Add(
                    new NamedInstance<IDeploymentTargetWorker>(notification.Worker, notification.Worker.TargetId));
            }

            StartWorker(notification.Worker, cancellationToken);

            return Task.CompletedTask;
        }

        public Task<Unit> Handle(StartWorker request, CancellationToken cancellationToken)
        {
            StartWorker(request.Worker, cancellationToken);

            return Task.FromResult(Unit.Value);
        }

        private DeploymentTargetWorker? GetWorkerByTargetId([NotNull] string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            if (!_configurationInstanceHolder.TryGet(targetId,
                out DeploymentTargetWorker worker))
            {
                int registered = _configurationInstanceHolder.RegisteredTypes
                    .Count(type => type == typeof(DeploymentTargetWorker));

                _logger.Warning("Could not get worker for target id {TargetId}, {Count} worker types registered",
                    targetId, registered);
            }

            return worker;
        }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            var foundWorker = GetWorkerByTargetId(deploymentTask.DeploymentTargetId);

            Task.Run<Task>(() => _mediator.Publish(new DeploymentTaskCreatedNotification(deploymentTask)));

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
            await Task.Yield();

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

                    foreach (string completedTaskKey in completedTaskKeys)
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

                await stoppingToken;
            }

            await Task.WhenAll(_tasks.Values.Where(task => !task.IsCompleted));
        }

        private void StartWorker(IDeploymentTargetWorker deploymentTargetWorker, CancellationToken stoppingToken)
        {
            try
            {
                TryStartWorker(deploymentTargetWorker, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not start worker for target id {TargetId}", deploymentTargetWorker.TargetId);
            }
        }

        private void TryStartWorker(IDeploymentTargetWorker deploymentTargetWorker, CancellationToken stoppingToken)
        {
            _logger.Debug("Trying to start worker for target id {TargetId}", deploymentTargetWorker.TargetId);

            if (_tasks.ContainsKey(deploymentTargetWorker.TargetId))
            {
                if (deploymentTargetWorker.IsRunning)
                {
                    _logger.Debug("Worker for target id {TargetId} is already running",
                        deploymentTargetWorker.TargetId);
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
            else
            {
                _logger.Debug("Start worker task was not found for target id {TargetId}",
                    deploymentTargetWorker.TargetId);
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cancellationTokenSource.Token);

            _cancellations.TryAdd(deploymentTargetWorker.TargetId, cancellationTokenSource);

            _tasks.Add(deploymentTargetWorker.TargetId,
                Task.Run(() => deploymentTargetWorker.ExecuteAsync(linked.Token), linked.Token));
        }

        private async Task StopWorkerAsync(DeploymentTargetWorker worker, CancellationToken cancellationToken)
        {
            _logger.Debug("Stopping worker for target id {TargetId}", worker.TargetId);
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