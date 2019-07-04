using System;
using System.Linq;
using Milou.Deployer.Web.Core.Extensions;
using Serilog.Events;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class LogEventLevelExtensions
    {
        public static LogEventLevel ParseOrDefault(this string levelText, LogEventLevel level = LogEventLevel.Information)
        {
            if (levelText.IsNullOrWhiteSpace())
            {
                return level;
            }

            if (!levelText.TryParse<LogEventLevel>(out var parsedLevel))
            {
                return level;
            }

            return parsedLevel;
        }
    }
}
