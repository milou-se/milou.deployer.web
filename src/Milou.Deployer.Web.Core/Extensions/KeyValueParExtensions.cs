using System.Collections.Generic;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class KeyValueParExtensions
    {
        public static KeyValuePair<string, string> MakeAnonymousValue(this KeyValuePair<string, string> pair)
        {
            return new KeyValuePair<string, string>(pair.Key, new string('*', 5));
        }
    }
}