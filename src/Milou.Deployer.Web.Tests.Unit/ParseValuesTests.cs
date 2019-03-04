using Milou.Deployer.Web.Core.Extensions;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class ParseValuesTests
    {
        [Fact]
        public void MakeAnonymousValues()
        {
            const string connectionString = "Server=localhost;password=p@ssword;user=root;";
            var anonymous = connectionString.MakeKeyValuePairAnonymous("user", "password");

            Assert.Equal("Server=localhost; password=*****; user=*****", anonymous);
        }

        [Fact]
        public void ShouldParseValues()
        {
            const string connectionString = "a=123;b=234;c=345;";
            var keyValuePairs = connectionString.ParseValues(';', '=');

            Assert.Equal(3, keyValuePairs.Length);
            Assert.Equal("123", keyValuePairs[0].Value);
            Assert.Equal("234", keyValuePairs[1].Value);
            Assert.Equal("345", keyValuePairs[2].Value);
        }
    }
}
