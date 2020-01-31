using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.IisHost.AspNetCore.TempData
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

            string key = typeof(T).FullName;

            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            try
            {
                tempData[key] = JsonConvert.SerializeObject(value);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public static T Get<T>(this ITempDataDictionary? tempData) where T : class
        {
            try
            {

                if (tempData is null)
                {
                    return default;
                }

                string key = typeof(T).FullName;

                if (string.IsNullOrWhiteSpace(key))
                {
                    return default;
                }

                tempData.TryGetValue(key, out var o);

                switch (o)
                {
                    case T item:
                        return item;
                    case string json:
                        try
                        {
                            if (typeof(T).IsAbstract)
                            {
                                return default;
                            }

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
            catch (Exception)
            {
                return null;
            }
        }
    }
}
