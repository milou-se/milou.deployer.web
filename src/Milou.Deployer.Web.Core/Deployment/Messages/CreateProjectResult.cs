using System.Collections.Immutable;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class CreateProjectResult
    {
        public CreateProjectResult(string projectName)
        {
            ProjectName = projectName;
            ValidationErrors = ImmutableArray<ValidationError>.Empty;
        }

        public CreateProjectResult(params ValidationError[] validationErrors)
        {
            ValidationErrors = validationErrors.SafeToImmutableArray();
        }

        [JsonConstructor]
        private CreateProjectResult(string projectName, ValidationError[] validationErrors)
        {
            ProjectName = projectName;
            ValidationErrors = validationErrors.ToImmutableArray();
        }

        public string ProjectName { get; }

        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}
