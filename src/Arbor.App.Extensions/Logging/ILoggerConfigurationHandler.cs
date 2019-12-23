using Serilog;

namespace Arbor.App.Extensions.Logging
{
    public interface ILoggerConfigurationHandler
    {
        LoggerConfiguration Handle(LoggerConfiguration loggerConfiguration);
    }
}