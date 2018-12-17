using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Milou.Deployer.Web.IisHost.Areas.Network;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class IpNetworkParserTests
    {
        [Fact]
        public void SingleIPv4AddressShouldBeParsed()
        {
            IpNetworkParser.TryParse("127.0.0.1/32", out IPNetwork network);

            Assert.NotNull(network);
            Assert.Equal(network.Prefix, IPAddress.Loopback);
            Assert.Equal(32, network.PrefixLength);
        }

        [Fact]
        public void NullAddressShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse(null, out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void EmptyAddressShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void MissingSeparatorShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("127.0.0.1", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void DoubleSeparatorsShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("127.0.0.1//32", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void InvalidIPv4AddressShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("256.0.0.1/32", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void NegativeIPv4NetworkLengthShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("127.0.0.1/-1", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void BadIPv4NetworkLengthShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("127.0.0.1/a", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void IPv4NetworkOutOfRangeLengthShouldNotBeParsed()
        {
            bool parsed = IpNetworkParser.TryParse("127.0.0.1/33", out IPNetwork network);

            Assert.False(parsed);
            Assert.Null(network);
        }

        [Fact]
        public void SingleIPv6AddressShouldBeParsed()
        {
            IpNetworkParser.TryParse("::1/32", out IPNetwork network);

            Assert.NotNull(network);
            Assert.Equal(IPAddress.Parse("::1"), network.Prefix);
            Assert.Equal(32, network.PrefixLength);
        }
    }
}