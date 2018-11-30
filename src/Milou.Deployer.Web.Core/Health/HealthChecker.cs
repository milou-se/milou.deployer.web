using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Milou.Deployer.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.Core.Health
{
    public class HealthChecker
    {
        private readonly IReadOnlyCollection<IHealthCheck> _healthChecks;
        private readonly ILogger _logger;

        public HealthChecker([NotNull] IReadOnlyCollection<IHealthCheck> healthChecks, [NotNull] ILogger logger)
        {
            _healthChecks = healthChecks ?? throw new ArgumentNullException(nameof(healthChecks));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DoHealthChecksAsync(CancellationToken cancellationToken)
        {
            if (_healthChecks.Count == 0)
            {
                _logger.Debug("No health checks are registered");
                return;
            }

            _logger.Debug("{HealthCheckCount} health checks are registered", _healthChecks.Count);

            foreach (IHealthCheck healthCheck in _healthChecks)
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(healthCheck.TimeoutInSeconds)))
                    {
                        using (CancellationTokenSource combined =
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