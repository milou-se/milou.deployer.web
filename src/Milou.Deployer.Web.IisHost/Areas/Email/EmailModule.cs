using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Email;

namespace Milou.Deployer.Web.IisHost.Areas.Email
{
    [UsedImplicitly]
    public class EmailModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmtpService>().AsImplementedInterfaces().SingleInstance();
        }
    }
}