using Serilog;

namespace Arbor.App.Extensions.Logging
{
    public interface IStartupLoggerConfigurationHandler
    {
        LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration);
    }
}