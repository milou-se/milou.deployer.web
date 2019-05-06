using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
using Milou.Deployer.Web.IisHost.Areas.AutoDeploy;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Messages;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public sealed class DeploymentTargetWorker : IDisposable
    {
        private readonly DeploymentService _deploymentService;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly BlockingCollection<DeploymentTask> _queue = new BlockingCollection<DeploymentTask>();
        private readonly BlockingCollection<DeploymentTask> _taskQueue = new BlockingCollection<DeploymentTask>();
        private readonly WorkerConfiguration _workerConfiguration;
        private readonly TimeoutHelper _timeoutHelper;
        public bool IsExecuting { get; private set; }

        public DeploymentTargetWorker(
            [NotNull] string targetId,
            [NotNull] DeploymentService deploymentService,
            [NotNull] ILogger logger,
            [NotNull] IMediator mediator,
            [NotNull] WorkerConfiguration workerConfiguration,
            TimeoutHelper timeoutHelper)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            TargetId = targetId;
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _workerConfiguration = workerConfiguration ?? throw new ArgumentNullException(nameof(workerConfiguration));
            _timeoutHelper = timeoutHelper;
        }

        public string TargetId { get; }

        private async Task StartTaskMessageHandler(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && IsExecuting)
            {
                var deploymentTask = _taskQueue.Take(stoppingToken);

                while (!deploymentTask.MessageQueue.IsCompleted)
                {
                    if (!deploymentTask.MessageQueue.TryTake(out (string Message, WorkTaskStatus Status) valueTuple,
                        TimeSpan.FromSeconds(_workerConfiguration.MessageTimeOutInSeconds)))
                    {
                        deploymentTask.MessageQueue.CompleteAdding();
                    }

                    if (valueTuple.Message.HasValue())
                    {
                        await _mediator.Publish(new DeploymentLogNotification(deploymentTask.DeploymentTargetId,
                                valueTuple.Message),
                            stoppingToken);
                    }
                }
            }
        }

        private DeploymentTask _currentTask;

        private async Task StartProcessingAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && IsExecuting)
            {
                var deploymentTask = _queue.Take(stoppingToken);

                if (!IsExecuting)
                {
                    return;
                }

                _currentTask = deploymentTask;

                deploymentTask.Status = WorkTaskStatus.Started;
                _taskQueue.Add(deploymentTask, stoppingToken);

                _logger.Information("Deployment target worker has taken {DeploymentTask}", deploymentTask);

                deploymentTask.Status = WorkTaskStatus.Started;

                _logger.Information("Executing deployment task {DeploymentTask}", deploymentTask);

                await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);

                var result =
                    await _deploymentService.ExecuteDeploymentAsync(deploymentTask, _logger, stoppingToken);

                if (result.ExitCode.IsSuccess)
                {
                    _logger.Information("Executed deployment task {DeploymentTask}", deploymentTask);

                    deploymentTask.Status = WorkTaskStatus.Done;
                    deploymentTask.Log("{\"Message\": \"Work task completed\"}");
                }
                else
                {
                    _logger.Error("Failed to deploy task {DeploymentTask}, result {Result}",
                        deploymentTask,
                        result.Metadata);

                    deploymentTask.Status = WorkTaskStatus.Failed;
                    deploymentTask.Log("{\"Message\": \"Work task failed\"}");
                }

                _currentTask = null;
            }

            while (_queue.Count > 0)
            {
                try
                {
                    _queue.Take();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not clear queue");
                }
            }
        }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            if (deploymentTask.DeploymentTargetId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(deploymentTask), "Target id is missing");
            }

            try
            {
                using (var cts = _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    var tasksInQueue = _queue.ToArray();

                    if (tasksInQueue.Length > 0 && tasksInQueue.Any(queued =>
                            queued.PackageId.Equals(deploymentTask.PackageId, StringComparison.OrdinalIgnoreCase)
                            && queued.SemanticVersion.Equals(deploymentTask.SemanticVersion)))
                    {
                        _logger.Warning(
                            "A deployment task with package id {PackageId} and version {Version} is already enqueued, skipping task, , current queue length {Length}",
                            deploymentTask.PackageId,
                            deploymentTask.SemanticVersion.ToNormalizedString(),
                            tasksInQueue.Length);

                        return;
                    }

                    if (deploymentTask.StartedBy != null
                        && deploymentTask.StartedBy.Equals(nameof(AutoDeployBackgroundService),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (tasksInQueue.Length > 0 && tasksInQueue.Any(queued => queued.StartedBy.Equals(
                                nameof(AutoDeployBackgroundService),
                                StringComparison.OrdinalIgnoreCase)))
                        {
                            return;
                        }

                        if (_currentTask != null && _currentTask.SemanticVersion == deploymentTask.SemanticVersion &&
                            _currentTask.PackageId == deploymentTask.PackageId)
                        {
                            return;
                        }
                    }

                    deploymentTask.Status = WorkTaskStatus.Enqueued;
                    _queue.Add(deploymentTask, cts.Token);

                    _logger.Information("Enqueued deployment task {DeploymentTask}, current queue length {Length}",
                        deploymentTask,
                        tasksInQueue);
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Failed to enqueue deployment task {DeploymentTask}", deploymentTask);
            }
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            IsExecuting = false;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (IsExecuting)
            {
                return;
            }

            IsExecuting = true;
            var messageTask = Task.Run(() => StartTaskMessageHandler(stoppingToken), stoppingToken);
            await StartProcessingAsync(stoppingToken);
            await messageTask;
        }

        public void Dispose()
        {
            IsExecuting = false;
            _queue?.Dispose();
            _taskQueue?.Dispose();
        }
    }
}
