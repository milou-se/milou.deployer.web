using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.Core
{
    [UsedImplicitly]
    public class DataSeederModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) =>
            builder.RegisterAssemblyTypesAsSingletons<IDataSeeder>(ApplicationAssemblies.FilteredAssemblies());
    }
}