using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
using Serilog;
using EnumerableExtensions = Milou.Deployer.Web.Core.Extensions.EnumerableExtensions;

namespace Milou.Deployer.Web.Core.Health
{
    public class HealthChecker
    {
        private readonly ImmutableArray<IHealthCheck> _healthChecks;
        private readonly ILogger _logger;
        private readonly TimeoutHelper _timeoutHelper;

        public HealthChecker(
            [NotNull] IEnumerable<IHealthCheck> healthChecks,
            [NotNull] ILogger logger,
            TimeoutHelper timeoutHelper)
        {
            _healthChecks = EnumerableExtensions.SafeToImmutableArray(healthChecks);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeoutHelper = timeoutHelper;
        }

        public async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
        {
            if (_healthChecks.Length == 0)
            {
                _logger.Debug("No health checks are registered");
                return;
            }

            _logger.Debug("{HealthCheckCount} health checks are registered", _healthChecks.Length);

            foreach (var healthCheck in _healthChecks)
            {
                try
                {
                    using (var cts =
                        _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(healthCheck.TimeoutInSeconds))
                    )
                    {
                        using (var combined =
                            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                        {
                            _logger.Debug("Making health check with {Check}", healthCheck.Description);
                            await healthCheck.CheckHealthAsync(combined.Token);
                        }
                    }
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Debug(ex, "Health check error for check {Check}", healthCheck.Description);
                }
            }

            _logger.Debug("Health checks done");
        }
    }
}
