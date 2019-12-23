using Arbor.App.Extensions.Logging;
using Serilog;

namespace Milou.Deployer.Web.Core.Logging
{
    public class LoggerConfigurationHandler : ILoggerConfigurationHandler
    {
        public LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration;
        }
    }
}