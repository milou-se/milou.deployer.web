using System;
using Milou.Deployer.Web.Core.Time;

namespace Milou.Deployer.Web.Tests.Unit
{
    internal class TestClock : ICustomClock
    {
        public DateTimeOffset UtcNow()
        {
            return new DateTime(2000,1,2,3,4,5,DateTimeKind.Utc);
        }

        public DateTime LocalNow()
        {
            return new DateTime(2000, 1, 2, 4, 4, 5, DateTimeKind.Local);
        }

        public DateTime ToLocalTime(DateTime dateTimeUtc)
        {
            DateTime localTime = dateTimeUtc.AddHours(-1);

            return new DateTime(localTime.Ticks, DateTimeKind.Local);
        }
    }
}