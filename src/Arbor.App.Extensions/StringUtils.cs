using System.Linq;

namespace Arbor.App.Extensions
{
    public static class StringUtils

    {
        public static bool AllHaveValues(params string[] values)
        {
            return values != null && values.All(paramValue => !string.IsNullOrWhiteSpace(paramValue));
        }
    }
}
