using System.Collections.Immutable;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class CreateTargetResult
    {
        public CreateTargetResult(string targetId, string targetName)
        {
            TargetId = targetId;
            TargetName = targetName;
            ValidationErrors = ImmutableArray<ValidationError>.Empty;
        }

        public CreateTargetResult(params ValidationError[] validationErrors)
        {
            ValidationErrors = validationErrors.SafeToImmutableArray();
        }

        [JsonConstructor]
        private CreateTargetResult(string targetName, ValidationError[] validationErrors)
        {
            TargetName = targetName;
            ValidationErrors = validationErrors.ToImmutableArray();
        }

        public string TargetId { get; }

        [PublicAPI]
        public string TargetName { get; }

        [PublicAPI]
        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}
