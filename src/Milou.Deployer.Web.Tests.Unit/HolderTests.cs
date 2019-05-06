using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class HolderTests
    {
        [Fact]
        public void CreateTypeWithSingletons()
        {
            var holder = new ConfigurationInstanceHolder();

            holder.AddInstance(new EnvironmentConfiguration { ApplicationBasePath = @"C:\app" });

            var environmentConsumer = holder.Create<EnvironmentConsumer>();

            Assert.NotNull(environmentConsumer);
            Assert.NotNull(environmentConsumer.EnvironmentConfiguration);
            Assert.Equal(@"C:\app", environmentConsumer.EnvironmentConfiguration.ApplicationBasePath);
        }
    }
}
