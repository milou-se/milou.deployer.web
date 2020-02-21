using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Arbor.App.Extensions;

namespace Milou.Deployer.Web.Agent
{
    public class DeploymentTaskPackage
    {
        public DeploymentTaskPackage(
            string deploymentTaskId,
            string deploymentTargetId,
            IEnumerable<string> deployerProcessArgs,
            string nugetConfigXml,
            string manifestJson,
            string publishSettingsXml,
            string agentId)
        {
            DeploymentTaskId = deploymentTaskId;
            DeployerProcessArgs = deployerProcessArgs.SafeToImmutableArray();
            DeploymentTargetId = deploymentTargetId;
            NugetConfigXml = nugetConfigXml;
            ManifestJson = manifestJson;
            PublishSettingsXml = publishSettingsXml;
            AgentId = agentId;
        }

        public ImmutableArray<string> DeployerProcessArgs { get; }

        [Required]
        public string DeploymentTargetId { get; }

        public string NugetConfigXml { get; }

        public string ManifestJson { get; }

        public string PublishSettingsXml { get; }

        public string AgentId { get; }

        [Required]
        public string DeploymentTaskId { get; }
    }
}