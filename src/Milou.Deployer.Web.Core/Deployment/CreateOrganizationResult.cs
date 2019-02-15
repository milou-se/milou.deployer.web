using System.Collections.Immutable;
using Arbor.KVConfiguration.Schema.Validators;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class CreateOrganizationResult
    {
        public ImmutableArray<ValidationError> ValidationErrors { get; }

        public CreateOrganizationResult(params ValidationError[] validationErrors)
        {
            ValidationErrors = validationErrors.SafeToImmutableArray();
        }
    }
}