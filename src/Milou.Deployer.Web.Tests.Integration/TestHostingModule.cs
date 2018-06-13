using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Tests.Integration
{
    [RegistrationOrder(int.MaxValue)]
    [UsedImplicitly]
    public class TestHostingModule : Module
    {
        private readonly EnvironmentConfiguration _environmentConfiguration;

        public TestHostingModule(EnvironmentConfiguration environmentConfiguration)
        {
            _environmentConfiguration = environmentConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            PortPoolRental availablePort = TcpHelper.GetAvailablePort(new PortPoolRange(5020, 5099));

            builder.RegisterInstance(availablePort);

            _environmentConfiguration.HttpPort = availablePort.Port;
        }
    }
}