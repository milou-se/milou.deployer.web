using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class TestHttpPort : IConfigureEnvironment
    {
        private readonly PortPoolRental _portPoolRental;

        public TestHttpPort(PortPoolRental portPoolRental)
        {
            _portPoolRental = portPoolRental;
        }

        public void Configure(EnvironmentConfiguration environmentConfiguration)
        {
            environmentConfiguration.HttpPort = _portPoolRental.Port;
        }
    }
}
