﻿using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class StandardServiceModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) => builder.AddSingleton<MonitoringService>(this);
    }
}