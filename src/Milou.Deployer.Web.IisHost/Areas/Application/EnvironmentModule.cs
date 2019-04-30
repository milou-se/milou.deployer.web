using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class EnvironmentModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.RegisterAssemblyTypesAsSingletons<IConfigureEnvironment>(Assemblies.FilteredAssemblies());
        }
    }
}
