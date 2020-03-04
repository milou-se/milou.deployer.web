﻿using System.Threading;
using System.Threading.Tasks;
using Marten;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten.DeploymentTasks
{
    public class DeploymentTaskPackageStore : IDeploymentTaskPackageStore
    {
        private readonly IDocumentStore _martenStore;

        public DeploymentTaskPackageStore(IDocumentStore martenStore) => _martenStore = martenStore;

        public async Task<DeploymentTaskPackage> GetDeploymentTaskPackageAsync(string deploymentTaskId,
            CancellationToken cancellationToken)
        {
            using var lightweightSession = _martenStore.LightweightSession();

            var found = await lightweightSession.Query<DeploymentTaskPackageData>()
                .SingleOrDefaultAsync(data => data.Id == deploymentTaskId);

            if (found is null)
            {
                return null;
            }

            return Map(found);
        }

        private DeploymentTaskPackage Map(DeploymentTaskPackageData data) =>
            new DeploymentTaskPackage(
                data.Id,
                data.DeploymentTargetId,
                data.ProcessArgs,
                data.NuGetConfigXml,
                data.ManifestJson,
                data.PublishSettingsXml,
                data.AgentId);
    }
}