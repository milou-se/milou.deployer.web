using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Configuration
{
    [UsedImplicitly]
    public class WorkerModule : Module
    {
        private readonly IEnumerable<DeploymentTargetWorker> _deploymentTargetWorkers;

        public WorkerModule([NotNull] IEnumerable<DeploymentTargetWorker> deploymentTargetWorkers)
        {
            _deploymentTargetWorkers = deploymentTargetWorkers ?? throw new ArgumentNullException(nameof(deploymentTargetWorkers));
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (DeploymentTargetWorker deploymentTargetWorker in _deploymentTargetWorkers)
            {
                builder.RegisterInstance(deploymentTargetWorker);
            }

            builder.RegisterType<DeploymentWorker>().AsSelf().SingleInstance();
        }
    }
}