using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class DeploymentWorker
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

        public DeploymentTargetWorker Worker([NotNull] string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetId));
            }

            return _workers.SingleOrDefault(worker => worker.TargetId.Equals(targetId));
        }

        public void Enqueue([NotNull] DeploymentTask deploymentTask)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            DeploymentTargetWorker foundWorker = Worker(deploymentTask.DeploymentTargetId);

            if (foundWorker is null)
            {
                throw new InvalidOperationException(
                    $"Could not find worker for deployment target id {deploymentTask.DeploymentTargetId}");
            }

            foundWorker.Enqueue(deploymentTask);
        }
    }
}