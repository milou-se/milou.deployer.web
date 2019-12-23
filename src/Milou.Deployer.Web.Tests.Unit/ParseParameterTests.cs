using Arbor.App.Extensions.Cli;
using Arbor.App.Extensions.Configuration;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class ParseParameterTests
    {
        [Fact]
        public void ShouldParseValue()
        {
            string[] args = { @"urn:milou:deployer:web:application-base-path=C:\Tools\Deployer\" };

            var result = args.ParseParameter(ConfigurationConstants.ApplicationBasePath);

            Assert.Equal(@"C:\Tools\Deployer\", result);
        }
    }
}
