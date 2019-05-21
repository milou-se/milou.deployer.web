using Milou.Deployer.Web.Core.Extensions;
using Serilog.Events;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class LogEventLevelExtensions
    {
        public static bool TryParse(this string levelText, out LogEventLevel level)
        {
            switch (levelText)
            {
                case nameof(LogEventLevel.Debug):
                    level = LogEventLevel.Debug;
                    return true;
                case nameof(LogEventLevel.Verbose):
                    level = LogEventLevel.Verbose;
                    return true;
                case nameof(LogEventLevel.Warning):
                    level = LogEventLevel.Warning;
                    return true;
                case nameof(LogEventLevel.Error):
                    level = LogEventLevel.Error;
                    return true;
                case nameof(LogEventLevel.Fatal):
                    level = LogEventLevel.Fatal;
                    return true;
            }

            level = LogEventLevel.Information;
            return false;
        }

        public static LogEventLevel ParseOrDefault(this string levelText, LogEventLevel level)
        {
            if (levelText.IsNullOrWhiteSpace())
            {
                return level;
            }

            if (!TryParse(levelText, out var parsedLevel))
            {
                return level;
            }

            return parsedLevel;
        }
    }
}
