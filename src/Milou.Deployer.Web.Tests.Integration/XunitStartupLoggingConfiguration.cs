using Arbor.App.Extensions.Logging;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class XunitStartupLoggingConfiguration : IStartupLoggerConfigurationHandler
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitStartupLoggingConfiguration(LoggingLevelSwitch levelSwitch, ITestOutputHelper testOutputHelper = null)
        {
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _testOutputHelper = testOutputHelper;
        }

        public LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration)
        {
            if (_testOutputHelper is null)
            {
                return loggerConfiguration;
            }

            return loggerConfiguration
                .WriteTo.TestOutput(_testOutputHelper)
                .WriteTo.Debug();
        }
    }
}
