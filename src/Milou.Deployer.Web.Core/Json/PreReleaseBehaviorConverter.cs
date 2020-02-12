using System;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Json
{
    public class PreReleaseBehaviorConverter : JsonConverter<PreReleaseBehavior>
    {
        public override void WriteJson(JsonWriter writer, [NotNull] PreReleaseBehavior value, JsonSerializer serializer) => writer?.WriteValue(value?.Name);

        public override PreReleaseBehavior ReadJson(JsonReader reader,
            Type objectType,
            PreReleaseBehavior existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) =>
            PreReleaseBehavior.Parse(reader?.Value?.ToString());
    }
}