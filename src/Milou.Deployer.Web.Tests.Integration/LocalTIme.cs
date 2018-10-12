using System;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class LocalTimeFormat
    {
        [Fact]
        public void ItShouldFormatUtcTimeAsLocal()
        {
            DateTime? dateTime = new DateTime(2000,3,5,7,11,13, DateTimeKind.Utc);

            var clock = new CustomSystemClock(timeZoneId:"W. Europe Standard Time");

            string formatted = dateTime.ToLocalTimeFormatted(clock);

            Assert.Equal("2000-03-05 08:11:13", formatted);
        }

        [Fact]
        public void ItShouldFormatUnspecifiedTimeAsLocal()
        {
            DateTime? dateTime = new DateTime(2000,3,5,7,11,13, DateTimeKind.Unspecified);

            var clock = new CustomSystemClock(timeZoneId:"W. Europe Standard Time");

            string formatted = dateTime.ToLocalTimeFormatted(clock);

            Assert.Equal("2000-03-05 08:11:13", formatted);
        }

    }
}