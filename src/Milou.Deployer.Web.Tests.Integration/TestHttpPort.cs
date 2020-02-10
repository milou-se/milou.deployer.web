﻿using System;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.Configuration;

namespace Milou.Deployer.Web.Tests.Integration
{
    public sealed class TestHttpPort : IConfigureEnvironment, IDisposable
    {
        public TestHttpPort(PortPoolRental portPoolRental)
        {
            Port = portPoolRental;
        }

        public PortPoolRental Port { get; }

        public void Configure(EnvironmentConfiguration environmentConfiguration)
        {
            environmentConfiguration.HttpPort = Port.Port;
        }

        public void Dispose()
        {
            Port?.Dispose();
        }
    }
}
