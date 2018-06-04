using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Json
{
    public static class JsonSerializerSettingsExtensions
    {
        public static JsonSerializerSettings UseCustomConverters(
            [NotNull] this JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            foreach (JsonConverter customConverter in JsonConverterHelper.GetCustomConverters())
            {
                serializerSettings.Converters.Add(customConverter);
            }

            return serializerSettings;
        }

        public static JsonSerializer UseCustomConverters(
            [NotNull] this JsonSerializer serializerSettings)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            foreach (JsonConverter customConverter in JsonConverterHelper.GetCustomConverters())
            {
                serializerSettings.Converters.Add(customConverter);
            }

            return serializerSettings;
        }
    }
}