using System;
using JetBrains.Annotations;
using Serilog;
using Serilog.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    public static class DelegateSinkExtensions
    {
        public static LoggerConfiguration DelegateSink(
            [NotNull] this LoggerSinkConfiguration loggerConfiguration,
            [NotNull] Action<string> action)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return loggerConfiguration.Sink(new DelegateSink(action));
        }
    }
}