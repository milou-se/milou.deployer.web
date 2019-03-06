using System.Linq;
using System.Net;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class IpAddressExtensions
    {
        public static bool EqualsAddress(this IPAddress address, IPAddress otherAddress)
        {
            if (address is null || otherAddress is null)
            {
                return false;
            }

            var sameFamily = address.AddressFamily == otherAddress.AddressFamily;

            if (!sameFamily)
            {
                return false;
            }

            return address.GetAddressBytes().SequenceEqual(otherAddress.GetAddressBytes());
        }
    }
}
