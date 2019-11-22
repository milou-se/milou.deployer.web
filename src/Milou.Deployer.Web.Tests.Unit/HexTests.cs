using System.Linq;
using System.Text;

using Milou.Deployer.Web.Core.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class HexTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HexTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [Fact]
        public void CanReadAndWriteHex()
        {
            var bytes = Encoding.UTF8.GetBytes("Abc");

            string hexString = bytes.FromByteArrayToHexString();

            _testOutputHelper.WriteLine(hexString);

            var convertedBytes = hexString.FromHexToByteArray();

            Assert.True(convertedBytes.SequenceEqual(bytes));
        }
    }
}