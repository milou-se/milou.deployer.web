using Arbor.App.Extensions.Cli;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class ParameterParserTests
    {
        [Theory]
        [InlineData("abc=123", "abc", "123")]
        [InlineData("abc=123", " abc", "123")]
        [InlineData("abc=123", "abc ", "123")]
        [InlineData("abc=123 ", "abc", "123")]
        [InlineData("abc=", "abc", "")]
        [InlineData("abc= ", "abc", "")]
        [InlineData("ABC=123", "abc", "123")]
        [InlineData("Abc=123", "abc", "123")]
        [InlineData("abc=123", "def", null)]
        [InlineData("abc", "abc", null)]
        [InlineData("", "abc", null)]
        [InlineData(" abc=123", "abc", "123")]
        [InlineData(null, "abc", null)]
        [InlineData("abc==", "abc", "=")]
        public void ValidArgShouldBeParsed(string parameter, string name, string expected)
        {
            string[] strings = { parameter };

            var actual = strings.ParseParameter(name);

            Assert.Equal(expected, actual);
        }
    }
}
