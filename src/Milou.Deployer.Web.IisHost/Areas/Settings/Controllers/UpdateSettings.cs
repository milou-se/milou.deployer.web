using System;

using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class UpdateSettings : IRequest<Unit>
    {
        public UpdateSettings(TimeSpan? cacheTime, NexusUpdate nexusConfig)
        {
            CacheTime = cacheTime;
            NexusConfig = nexusConfig;
        }

        public TimeSpan? CacheTime { get; }

        public NexusUpdate NexusConfig { get; }
    }
}