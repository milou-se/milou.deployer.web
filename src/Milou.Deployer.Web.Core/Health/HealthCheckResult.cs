using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Health
{
    public class HealthCheckResult
    {
        public HealthCheckResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        [PublicAPI]
        public bool Succeeded { get; }
    }
}