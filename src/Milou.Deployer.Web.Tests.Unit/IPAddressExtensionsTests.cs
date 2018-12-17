using System.Net;
using Milou.Deployer.Web.Core;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class IPAddressExtensionsTests
    {
        [Fact]
        public void DifferentAddressesFamiliesShouldNotBeEqual()
        {
            bool equal = IPAddressExtensions.EqualsAddress(IPAddress.Parse("::1"), IPAddress.Parse("127.0.0.1"));

            Assert.False(equal);
        }

        [Fact]
        public void DifferentIPv4AddressesShouldNotBeEqual()
        {
            bool equal = IPAddressExtensions.EqualsAddress(IPAddress.Parse("192.168.0.1"), IPAddress.Parse("192.168.0.2"));

            Assert.False(equal);
        }

        [Fact]
        public void FirstNullAddressShouldNotBeEqual()
        {
            bool equal = IPAddressExtensions.EqualsAddress(null, IPAddress.Parse("127.0.0.1"));

            Assert.False(equal);
        }

        [Fact]
        public void LoopBackShouldBeEqual()
        {
            bool equal = IPAddressExtensions.EqualsAddress(IPAddress.Loopback, IPAddress.Loopback);

            Assert.True(equal);
        }

        [Fact]
        public void SecondNullAddressShouldNotBeEqual()
        {
            bool equal = IPAddressExtensions.EqualsAddress(IPAddress.Parse("127.0.0.1"), null);

            Assert.False(equal);
        }
    }
}