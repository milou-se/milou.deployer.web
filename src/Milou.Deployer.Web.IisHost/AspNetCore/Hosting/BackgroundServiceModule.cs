using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.DependencyInjection;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    [UsedImplicitly]
    public class BackgroundServiceModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            var types = ApplicationAssemblies.FilteredAssemblies().GetLoadablePublicConcreteTypesImplementing<IHostedService>();

            foreach (Type type in types)
            {
                builder.AddSingleton(type, this);
                builder.AddSingleton<IHostedService>(context => context.GetService(type), this);
            }

            return builder;
        }
    }
}
