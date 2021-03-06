﻿using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Tests.Integration
{
    [RegistrationOrder(int.MaxValue)]
    [UsedImplicitly]
    public class TestHostingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            PortPoolRental availablePort = TcpHelper.GetAvailablePort(new PortPoolRange(5020, 5099));
            builder.RegisterInstance(availablePort);
        }
    }
}