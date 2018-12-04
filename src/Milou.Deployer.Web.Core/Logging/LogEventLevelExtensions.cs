using Milou.Deployer.Web.Core.Extensions;
using Serilog.Events;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class LogEventLevelExtensions
    {
        public static LogEventLevel ParseOrDefault(this string levelText, LogEventLevel level)
        {
            if (levelText.IsNullOrWhiteSpace())
            {
                return level;
            }

            switch (levelText)
            {
                case nameof(LogEventLevel.Debug):
                    return LogEventLevel.Debug;
                case nameof(LogEventLevel.Verbose):
                    return LogEventLevel.Verbose;
                case nameof(LogEventLevel.Warning):
                    return LogEventLevel.Warning;
                case nameof(LogEventLevel.Error):
                    return LogEventLevel.Error;
                case nameof(LogEventLevel.Fatal):
                    return LogEventLevel.Fatal;

                default:
                    return LogEventLevel.Information;
            }
        }
    }
}