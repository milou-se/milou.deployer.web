using System;
using System.Linq;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    public static class StringHexExtensions
    {
        public static byte[] ToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}