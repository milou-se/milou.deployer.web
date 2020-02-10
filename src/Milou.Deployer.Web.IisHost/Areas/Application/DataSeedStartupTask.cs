using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Time;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Startup;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class DataSeedStartupTask : BackgroundService, IStartupTask
    {
        private readonly IKeyValueConfiguration _configuration;
        private readonly ImmutableArray<IDataSeeder> _dataSeeders;
        private readonly ILogger _logger;
        private readonly TimeoutHelper _timeoutHelper;

        public DataSeedStartupTask(
            IEnumerable<IDataSeeder> dataSeeders,
            IKeyValueConfiguration configuration,
            ILogger logger,
            TimeoutHelper timeoutHelper)
        {
            _dataSeeders = dataSeeders.SafeToImmutableArray();
            _configuration = configuration;
            _logger = logger;
            _timeoutHelper = timeoutHelper;
        }

        public bool IsCompleted { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken startupCancellationToken)
        {
            await Task.Yield();

            if (_dataSeeders.Length > 0)
            {
                _logger.Debug("Running data seeders");

                await Task.Run(() => RunSeeders(startupCancellationToken), startupCancellationToken);

            }
            else
            {
                _logger.Debug("No data seeders were found");
            }

        }

        private async Task RunSeeders(CancellationToken cancellationToken)
        {
            if (!int.TryParse(_configuration[DeployerAppConstants.SeedTimeoutInSeconds],
                    out var seedTimeoutInSeconds) ||
                seedTimeoutInSeconds <= 0)
            {
                seedTimeoutInSeconds = 10;
            }

            foreach (var dataSeeder in _dataSeeders)
            {
                try
                {
                    using (var startupToken =
                        _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(seedTimeoutInSeconds)))
                    {
                        using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                            cancellationToken,
                            startupToken.Token))
                        {
                            _logger.Debug("Running data seeder {Seeder}", dataSeeder.GetType().FullName);
                            await dataSeeder.SeedAsync(linkedToken.Token);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    _logger.Error(ex, "Could not run seeder {Seeder}", dataSeeder.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to run seeder {Seeder}", dataSeeder.GetType().Name);
                }
            }

            IsCompleted = true;

            _logger.Debug("Done running data seeders");
        }
    }
}
