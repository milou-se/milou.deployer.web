using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Startup;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class StartupTaskModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            var startupTaskTypes = ApplicationAssemblies.FilteredAssemblies()
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(t => t.IsPublicConcreteTypeImplementing<IStartupTask>());

            foreach (var startupTask in startupTaskTypes)
            {
                builder.AddSingleton<IStartupTask>(context => context.GetService(startupTask), this);

                if (builder.Any(serviceDescriptor => serviceDescriptor.ImplementationType == startupTask
                                                     && serviceDescriptor.ServiceType == startupTask))
                {
                    continue;
                }

                builder.AddSingleton(startupTask, this);
            }

            return builder;
        }
    }
}
