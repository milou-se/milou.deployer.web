using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.DependencyInjection;
using Arbor.App.Extensions.ExtensionMethods;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    public class HealthCheckModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            foreach (var type in ApplicationAssemblies.FilteredAssemblies()
                .GetLoadablePublicConcreteTypesImplementing<IHealthCheck>())
            {
                builder.AddSingleton(type, this);
            }

            return builder.AddSingleton<HealthChecker>(this);
        }
    }
}
