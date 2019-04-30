using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class StartupTaskModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            var startupTaskTypes = Assemblies.FilteredAssemblies().SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsPublicConcreteTypeImplementing<IStartupTask>());

            foreach (var startupTask in startupTaskTypes)
            {
                builder.AddSingleton(typeof(IHostedService), context => context.GetService(startupTask), this);
                builder.AddSingleton(typeof(IStartupTask), context => context.GetService(startupTask), this);
                builder.AddSingleton(startupTask);
            }

            return builder;
        }
    }
}
