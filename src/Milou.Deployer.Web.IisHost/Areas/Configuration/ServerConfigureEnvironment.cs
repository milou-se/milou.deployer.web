using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.Configuration;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    [UsedImplicitly]
    public class ServerConfigureEnvironment : IConfigureEnvironment
    {
        public void Configure(EnvironmentConfiguration environmentConfiguration) =>
            environmentConfiguration.HttpEnabled = true;
    }
}