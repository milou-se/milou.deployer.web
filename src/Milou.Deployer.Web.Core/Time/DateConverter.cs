using System;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Time
{
    public class DateConverter : JsonConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            return new Date(DateTime.Parse(reader.Value.ToString()));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Date)value).ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Date);
        }
    }
}
