using System.Net;
using Milou.Deployer.Web.IisHost.Areas.Network;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class IpNetworkParserTests
    {
        [Fact]
        public void BadIPv4NetworkLengthShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("127.0.0.1/a", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void DoubleSeparatorsShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("127.0.0.1//32", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void EmptyAddressShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void InvalidIpv4AddressShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("256.0.0.1/32", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void Ipv4NetworkOutOfRangeLengthShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("127.0.0.1/33", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void MissingSeparatorShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("127.0.0.1", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void NegativeIPv4NetworkLengthShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse("127.0.0.1/-1", out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void NullAddressShouldNotBeParsed()
        {
            var parsed = IpNetworkParser.TryParse(null, out var network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void SingleIPv4AddressShouldBeParsed()
        {
            _ = IpNetworkParser.TryParse("127.0.0.1/32", out var network);

            Assert.NotNull(network);
            Assert.Equal(network.Prefix, IPAddress.Loopback);
            Assert.Equal(32, network.PrefixLength);
        }

        [Fact]
        public void SingleIPv6AddressShouldBeParsed()
        {
            _ = IpNetworkParser.TryParse("::1/32", out var network);

            Assert.NotNull(network);
            Assert.Equal(IPAddress.Parse("::1"), network.Prefix);
            Assert.Equal(32, network.PrefixLength);
        }
    }
}
