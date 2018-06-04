using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Targets;
using Serilog;

namespace Milou.Deployer.Web.Tests.Integration
{
    [RegistrationOrder(int.MaxValue, Tag=Scope.AspNetCoreScope)]
    [UsedImplicitly]
    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new InMemoryDeploymentTargetReadService(context.Resolve<ILogger>(), TestDataCreator.CreateData)).AsImplementedInterfaces().SingleInstance();
        }
    }
}