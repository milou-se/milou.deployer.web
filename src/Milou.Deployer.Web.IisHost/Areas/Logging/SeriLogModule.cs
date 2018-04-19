using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    [UsedImplicitly]
    public class SeriLogModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(context => Serilog.Log.Logger)
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}