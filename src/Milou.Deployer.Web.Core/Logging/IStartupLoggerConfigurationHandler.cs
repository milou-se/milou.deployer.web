using Serilog;

namespace Milou.Deployer.Web.Core.Logging
{
    public interface IStartupLoggerConfigurationHandler
    {
        LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration);
    }
}