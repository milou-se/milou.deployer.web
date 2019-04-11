using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Health
{
    public interface IHealthCheck
    {
        int TimeoutInSeconds { get; }

        string Description { get; }

        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken);
    }
}
