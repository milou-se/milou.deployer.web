using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class EnvironmentModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assemblies.FilteredAssemblies().ToArray())
                .Where(type => type.IsPublicConcreteTypeImplementing<IConfigureEnvironment>())
                .As<IConfigureEnvironment>();
        }
    }
}
