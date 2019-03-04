using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    [RegistrationOrder(0, Tag = Scope.AspNetCoreScope)]
    public class HealthCheckModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = Assemblies.FilteredAssemblies().ToArray();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => type.IsConcreteTypeImplementing<IHealthCheck>())
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<HealthChecker>().AsSelf().SingleInstance();
        }
    }
}
