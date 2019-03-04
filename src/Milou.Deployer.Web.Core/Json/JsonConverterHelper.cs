using System.Collections.Generic;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Json
{
    public static class JsonConverterHelper
    {
        internal static IEnumerable<JsonConverter> GetCustomConverters()
        {
            yield return new StringValuesJsonConverter();
            yield return new DateConverter();
            yield return new EnvironmentTypeConverter();
        }
    }
}
