using System.Collections.Immutable;
using Arbor.KVConfiguration.Schema.Validators;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class UpdateDeploymentTargetResult
    {
        public UpdateDeploymentTargetResult(params ValidationError[] validationErrors)
        {
            ValidationErrors = validationErrors.ToImmutableArray();
        }

        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}