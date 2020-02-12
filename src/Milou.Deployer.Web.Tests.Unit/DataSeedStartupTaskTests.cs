using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Time;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Unit
{
    public sealed class DataSeedStartupTaskTests : IDisposable
    {
        public DataSeedStartupTaskTests(ITestOutputHelper output) => _logger = output.FromTestOutput();

        public void Dispose()
        {
            _logger.SafeDispose();
            _startupTask.SafeDispose();
        }

        private readonly ILogger _logger;
        private DataSeedStartupTask _startupTask;

        [Fact]
        public async Task RunSeedersEmpty()
        {
            IKeyValueConfiguration configuration = new InMemoryKeyValueConfiguration(new NameValueCollection());

            _startupTask = new DataSeedStartupTask(
                ImmutableArray<IDataSeeder>.Empty,
                configuration,
                _logger,
                new TimeoutHelper());

            await _startupTask.StartAsync(CancellationToken.None);

            Assert.True(_startupTask.IsCompleted);
        }
    }
}