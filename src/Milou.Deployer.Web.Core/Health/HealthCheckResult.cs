namespace Milou.Deployer.Web.Core.Health
{
    public class HealthCheckResult
    {
        public HealthCheckResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public bool Succeeded { get; }
    }
}