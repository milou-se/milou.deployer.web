using System.Collections.Immutable;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Schema.Validators;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class UpdateDeploymentTargetResult
    {
        public UpdateDeploymentTargetResult(params ValidationError[] validationErrors)
        {
            ValidationErrors = validationErrors.ToImmutableArray();
        }

        [PublicAPI]
        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}
