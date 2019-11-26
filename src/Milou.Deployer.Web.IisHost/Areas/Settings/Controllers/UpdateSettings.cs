using System;

using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class UpdateSettings : IRequest<Unit>
    {
        public UpdateSettings(TimeSpan? cacheTime, NexusUpdate nexusConfig, AutoDeployUpdate autoDeploy)
        {
            CacheTime = cacheTime;
            NexusConfig = nexusConfig;
            AutoDeploy = autoDeploy;
        }

        public TimeSpan? CacheTime { get; }

        public NexusUpdate NexusConfig { get; }

        public AutoDeployUpdate AutoDeploy { get; }
    }
}