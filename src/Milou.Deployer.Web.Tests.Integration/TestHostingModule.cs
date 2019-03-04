using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Tests.Integration
{
    [RegistrationOrder(int.MaxValue)]
    [UsedImplicitly]
    public class TestHostingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var availablePort = TcpHelper.GetAvailablePort(new PortPoolRange(5020, 100));
            builder.RegisterInstance(availablePort);
        }
    }
}
