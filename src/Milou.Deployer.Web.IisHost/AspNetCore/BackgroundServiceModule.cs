using System.Linq;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Module = Autofac.Module;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [RegistrationOrder(int.MaxValue, Tag = Scope.AspNetCoreScope)]
    [UsedImplicitly]
    public class BackgroundServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Assembly[] assemblies = Assemblies.FilteredAssemblies().ToArray();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => type.IsConcreteTypeImplementing<IHostedService>())
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}