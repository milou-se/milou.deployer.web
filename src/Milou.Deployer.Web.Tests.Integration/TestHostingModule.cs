using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Configuration;

namespace Milou.Deployer.Web.Tests.Integration
{
    [RegistrationOrder(int.MaxValue)]
    [UsedImplicitly]
    public class TestHostingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            int availablePort = TcpHelper.GetAvailablePort(new PortPoolRange(5000, 5099));

            builder.RegisterInstance(new EnvironmentConfiguration { HttpPort = availablePort });
        }
    }
}