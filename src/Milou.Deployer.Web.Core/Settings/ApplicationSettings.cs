using System;

using Milou.Deployer.Web.Core.Integration.Nexus;

namespace Milou.Deployer.Web.Core.Settings
{
    public class ApplicationSettings
    {
        public TimeSpan CacheTime { get; set; }

        public NexusConfig NexusConfig { get; set; } = new NexusConfig();

        public AutoDeploySettings AutoDeploy { get; set; }
    }
}