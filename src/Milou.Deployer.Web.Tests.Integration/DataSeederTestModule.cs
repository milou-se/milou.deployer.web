using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.DependencyInjection;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class DataSeederTestModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.RegisterAssemblyTypesAsSingletons<IDataSeeder>(ApplicationAssemblies.FilteredAssemblies());
        }
    }
}
