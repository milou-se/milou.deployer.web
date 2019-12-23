using Serilog;

namespace Milou.Deployer.Web.Core.Logging
{
    public interface ILoggerConfigurationHandler
    {
        LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration);
    }
}