using Serilog;

namespace Milou.Deployer.Web.Core.Logging
{
    public class StartupLoggerConfigurationHandler : IStartupLoggerConfigurationHandler
    {
        public LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration;
        }
    }
}