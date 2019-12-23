﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Arbor.App.Extensions.Configuration;
using Milou.Deployer.Web.IisHost;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AppTest
    {
        public AppTest(ITestOutputHelper output) => _output = output;

        private readonly ITestOutputHelper _output;

        [Fact(Skip = "Full app test")]
        public async Task RunApplicationWithExitCode0()
        {
            var envArgs = new Dictionary<string, string> { [ConfigurationConstants.RestartTimeInSeconds] = "20" }
                .ToImmutableDictionary();

            var exitCode = await AppStarter.StartAsync(Array.Empty<string>(), envArgs, _output);

            Assert.Equal(0, exitCode);
        }
    }
}
