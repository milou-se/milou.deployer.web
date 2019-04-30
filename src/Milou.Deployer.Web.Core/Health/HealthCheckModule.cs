using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    public class HealthCheckModule : Module
    {
        //protected override void Load(ContainerBuilder builder)
        //{
        //    var assemblies = Assemblies.FilteredAssemblies().ToArray();

        //    builder.RegisterAssemblyTypes(assemblies)
        //        .Where(type => type.IsConcreteTypeImplementing<IHealthCheck>())
        //        .AsImplementedInterfaces().SingleInstance();

        //    builder.RegisterType<HealthChecker>().AsSelf().SingleInstance();
        //}
    }
}
