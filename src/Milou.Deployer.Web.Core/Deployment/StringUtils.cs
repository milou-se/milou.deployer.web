using System.Linq;

namespace Milou.Deployer.Web.Core.Deployment
{
    public static class StringUtils

    {
        public static bool AllHasValue(params string[] values)
        {
            return values != null && values.All(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}