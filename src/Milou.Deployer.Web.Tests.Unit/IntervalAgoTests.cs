﻿using System;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class IntervalAgoTests
    {
        [Fact]
        public void Ago()
        {
            DateTime? utcTime = new DateTime(2000,1,2,5,0,0,DateTimeKind.Utc);

            DeploymentInterval deploymentInterval = utcTime.IntervalAgo(new TestClock());

            Assert.Equal(DeploymentInterval.ThisWeek, deploymentInterval);
        }
    }
}