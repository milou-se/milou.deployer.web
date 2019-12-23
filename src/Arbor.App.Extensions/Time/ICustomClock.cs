using System;

namespace Arbor.App.Extensions.Time
{
    public interface ICustomClock
    {
        DateTimeOffset UtcNow();

        DateTime LocalNow();

        DateTime ToLocalTime(DateTime dateTimeUtc);
    }
}
