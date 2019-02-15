using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class DeploymentWorker : BackgroundService
    {
        private ImmutableArray<DeploymentTargetWorker> _workers;

        public DeploymentWorker([NotNull] IEnumerable<DeploymentTargetWorker> workers)
        {
            if (workers == null)
            {
                throw new ArgumentNullException(nameof(workers));
            }

            _workers = workers.ToImmutableArray();
        }

        private DeploymentTargetWorker GetWorkerByTargetId([NotNull] string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            if (_workers.Length == 0)
            {
                throw new Core.DeployerAppException("There are no registered deployment workers");
            }

            DeploymentTargetWorker workerByTargetId = _workers.SingleOrDefault(worker => worker.TargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase));

            return workerByTargetId;
        }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            DeploymentTargetWorker foundWorker = GetWorkerByTargetId(deploymentTask.DeploymentTargetId);

            if (foundWorker is null)
            {
                throw new Core.DeployerAppException(
                    $"Could not find worker for deployment target id {deploymentTask.DeploymentTargetId}");
            }

            foundWorker.Enqueue(deploymentTask);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>(_workers.Length);

            foreach (DeploymentTargetWorker deploymentTargetWorker in _workers)
            {
                tasks.Add(Task.Run(() => deploymentTargetWorker.ExecuteAsync(stoppingToken), stoppingToken));
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1),stoppingToken);
            }

            await Task.WhenAll(tasks);
        }
    }
}