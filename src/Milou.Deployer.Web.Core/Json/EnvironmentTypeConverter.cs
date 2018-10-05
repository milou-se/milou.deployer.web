using System;
using Milou.Deployer.Web.Core.Deployment;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Json
{
    public class EnvironmentTypeConverter : JsonConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            return  EnvironmentType.Parse(reader.Value as string);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is EnvironmentType environmentType)
            {
                writer.WriteValue(environmentType.Name);
                return;
            }

            throw new DeployerAppException($"Type must be {typeof(EnvironmentType).FullName}");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EnvironmentType);
        }
    }
}