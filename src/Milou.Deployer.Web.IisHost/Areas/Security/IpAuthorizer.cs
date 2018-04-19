using System;
using System.Linq;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public static class IpAuthorizer
    {
        public static bool IsAuthorized(string remoteIp, IpPolicy ipPolicy)
        {
            string[] whiteInternalListed =
            {
                "192.168.",
                "172.16.",
                "10."
            };

            string[] privateAddresses =
            {
                "::1",
                "127.0.0.1"
            };

            if (string.IsNullOrWhiteSpace(remoteIp))
            {
                return false;
            }

            if (ipPolicy == IpPolicy.External)
            {
                return true;
            }

            bool isIncludedPrivate = privateAddresses.Any(
                whiteListedAddress =>
                    remoteIp.Equals(whiteListedAddress, StringComparison.InvariantCultureIgnoreCase));

            if (isIncludedPrivate)
            {
                return true;
            }

            bool isIncluded = whiteInternalListed.Any(
                whiteListedAddress =>
                    remoteIp.StartsWith(whiteListedAddress, StringComparison.InvariantCultureIgnoreCase));

            return isIncluded;
        }
    }
}