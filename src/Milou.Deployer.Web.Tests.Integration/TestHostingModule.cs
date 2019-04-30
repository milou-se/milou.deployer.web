using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class TestHostingModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            var availablePort = TcpHelper.GetAvailablePort(new PortPoolRange(5020, 100));
            return builder.AddSingleton(availablePort);
        }
    }
}
