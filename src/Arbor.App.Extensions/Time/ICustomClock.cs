using System;

namespace Milou.Deployer.Web.Core.Time
{
    public interface ICustomClock
    {
        DateTimeOffset UtcNow();

        DateTime LocalNow();

        DateTime ToLocalTime(DateTime dateTimeUtc);
    }
}
