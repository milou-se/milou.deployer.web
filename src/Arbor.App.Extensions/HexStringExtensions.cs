using System;
using System.Linq;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class HexStringExtensions
    {
        public static byte[] FromHexToByteArray(this string hex) =>
            Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();

        public static string FromByteArrayToHexString(this byte[] bytes) => string.Concat(bytes.Select(b => b.ToString("X2")));
    }
}