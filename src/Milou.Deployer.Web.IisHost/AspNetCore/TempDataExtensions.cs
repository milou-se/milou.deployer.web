using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public static class TempDataExtensions
    {
        public static void Put<T>([NotNull] this ITempDataDictionary tempData, T value) where T : class
        {
            if (tempData is null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            if (value is null)
            {
                return;
            }

            var key = typeof(T).FullName;

            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>([NotNull] this ITempDataDictionary tempData) where T : class
        {
            if (tempData is null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            var key = typeof(T).FullName;

            tempData.TryGetValue(key, out var o);

            switch (o)
            {
                case T item:
                    return item;
                case string json:
                    try
                    {
                        var deserializeObject = JsonConvert.DeserializeObject<T>(json);

                        return deserializeObject;
                    }
                    catch (Exception)
                    {
                        return default;
                    }

                default:
                    return default;
            }
        }
    }
}
