using Arbor.App.Extensions.Application;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Tests.Unit
{
    [UsedImplicitly]
    internal class EnvironmentConsumer
    {
        public EnvironmentConsumer(EnvironmentConfiguration environmentConfiguration)
        {
            EnvironmentConfiguration = environmentConfiguration;
        }

        public EnvironmentConfiguration EnvironmentConfiguration { get; }
    }
}