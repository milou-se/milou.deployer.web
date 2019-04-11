using System.Net;
using Milou.Deployer.Web.Core.Extensions;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class IpAddressExtensionsTests
    {
        [Fact]
        public void DifferentAddressesFamiliesShouldNotBeEqual()
        {
            var equal = IPAddress.Parse("::1").EqualsAddress(IPAddress.Parse("127.0.0.1"));

            Assert.False(equal);
        }

        [Fact]
        public void DifferentIPv4AddressesShouldNotBeEqual()
        {
            var equal = IPAddress.Parse("192.168.0.1").EqualsAddress(IPAddress.Parse("192.168.0.2"));

            Assert.False(equal);
        }

        [Fact]
        public void FirstNullAddressShouldNotBeEqual()
        {
            var equal = IpAddressExtensions.EqualsAddress(null, IPAddress.Parse("127.0.0.1"));

            Assert.False(equal);
        }

        [Fact]
        public void LoopBackShouldBeEqual()
        {
            var equal = IPAddress.Loopback.EqualsAddress(IPAddress.Loopback);

            Assert.True(equal);
        }

        [Fact]
        public void SecondNullAddressShouldNotBeEqual()
        {
            var equal = IPAddress.Parse("127.0.0.1").EqualsAddress(null);

            Assert.False(equal);
        }
    }
}
