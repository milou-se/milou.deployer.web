using System.Collections.Immutable;
using Arbor.App.Extensions;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class CreateOrganizationResult
    {
        public CreateOrganizationResult(params ValidationError[] validationErrors)
        {
            ValidationErrors = validationErrors.SafeToImmutableArray();
        }

        public ImmutableArray<ValidationError> ValidationErrors { get; }
    }
}
