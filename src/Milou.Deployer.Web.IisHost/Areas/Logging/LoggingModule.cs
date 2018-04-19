using Autofac;

namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    public class LoggingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => Serilog.Log.Logger).SingleInstance().AsImplementedInterfaces();
        }
    }
}