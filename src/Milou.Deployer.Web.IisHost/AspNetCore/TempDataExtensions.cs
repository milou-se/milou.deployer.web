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
            if (tempData == null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            if (value is null)
            {
                return;
            }

            string key = typeof(T).FullName;

            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>([NotNull] this ITempDataDictionary tempData) where T : class
        {
            if (tempData == null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            string key = typeof(T).FullName;

            tempData.TryGetValue(key, out object o);

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
                        return null;
                    }

                default:
                    return null;
            }
        }
    }
}