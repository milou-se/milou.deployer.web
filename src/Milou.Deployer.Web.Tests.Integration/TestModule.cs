﻿using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.DependencyInjection;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Time;
using Serilog;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class TestModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder
                .AddSingleton<IDeploymentTargetReadService>(context =>
                        new InMemoryDeploymentTargetReadService(context.GetService<ILogger>(),
                            TestDataCreator.CreateData),
                    this)
                .AddSingleton(new TimeoutConfiguration { CancellationEnabled = false });
        }
    }
}
