using Arbor.App.Extensions.Configuration;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class ConfigurationKeyAnonymousTest
    {
        [Fact]
        public void PasswordShouldBeHidden()
        {
            var configurationKeyInfo = new ConfigurationKeyInfo("password", "abc123", null);

            Assert.Equal("password", configurationKeyInfo.Key);
            Assert.Equal("*****", configurationKeyInfo.Value);
        }
    }
}
