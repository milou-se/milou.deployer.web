using System;
using Arbor.App.Extensions.Time;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Json
{
    public class DateConverter : JsonConverter
    {
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader?.Value is null)
            {
                return null;
            }

            return new Date(DateTime.Parse(reader.Value.ToString()));
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }
            writer.WriteValue(((Date)value).ToString());
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(Date);
    }
}
