using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Processing;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class DeploymentTargetWorker
    {
        private readonly DeploymentService _deploymentService;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly BlockingCollection<DeploymentTask> _queue = new BlockingCollection<DeploymentTask>();

        private readonly BlockingCollection<DeploymentTask> _runningTasks = new BlockingCollection<DeploymentTask>();
        private WorkerConfiguration _workerConfiguration;

        public DeploymentTargetWorker(
            [NotNull] string targetId,
            [NotNull] DeploymentService deploymentService,
            [NotNull] ILogger logger,
            [NotNull] IMediator mediator,
            WorkerConfiguration workerConfiguration)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            TargetId = targetId;
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _workerConfiguration = workerConfiguration;
        }

        public string TargetId { get; }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            deploymentTask.Status = WorkTaskStatus.Enqueued;
            _queue.Add(deploymentTask);
            _logger.Information("Enqueued deployment task {DeploymentTask}", deploymentTask);
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task messageTask = Task.Run(() => StartTaskMessageHandler(stoppingToken), stoppingToken);
            await StartProcessingAsync(stoppingToken);
            await messageTask;
        }

        private async Task StartTaskMessageHandler(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DeploymentTask deploymentTask = _runningTasks.Take(stoppingToken);

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

        private async Task StartProcessingAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DeploymentTask deploymentTask = _queue.Take(stoppingToken);

                deploymentTask.Status = WorkTaskStatus.Started;
                _runningTasks.Add(deploymentTask, stoppingToken);

                _logger.Information("Deployment target worker has taken {DeploymentTask}", deploymentTask);

                deploymentTask.Status = WorkTaskStatus.Started;

                _logger.Information("Executing deployment task {DeploymentTask}", deploymentTask);

                await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);

                (ExitCode ExitCode, string Log) result =
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
                        result.Log);

                    deploymentTask.Status = WorkTaskStatus.Failed;
                    deploymentTask.Log("{\"Message\": \"Work task failed\"}");
                }
            }

            _queue?.Dispose();
        }
    }
}