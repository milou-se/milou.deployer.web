using System.Collections.Immutable;
using Arbor.KVConfiguration.Schema.Validators;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class CreateTargetResult : ApiResult
    {
        public CreateTargetResult(string targetName)
        {
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

        public string TargetName { get; }

        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}