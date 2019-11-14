using System;
using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class UpdateSettings :IRequest<Unit>
    {
        public TimeSpan? CacheTime { get; }

        public UpdateSettings(TimeSpan? cacheTime)
        {
            CacheTime = cacheTime;
        }
    }
}
