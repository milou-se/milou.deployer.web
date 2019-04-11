using System.Linq;

namespace Milou.Deployer.Web.Core.Deployment
{
    public static class StringUtils

    {
        public static bool AllHaveValues(params string[] values)
        {
            return values != null && values.All(paramValue => !string.IsNullOrWhiteSpace(paramValue));
        }
    }
}
