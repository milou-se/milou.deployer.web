using System;
using System.Collections.Concurrent;
using System.Threading;
using Serilog;

namespace Arbor.App.Extensions.Logging
{
    public static class TempLogger
    {
        private static readonly ConcurrentQueue<string> LogMessages = new ConcurrentQueue<string>();

        public static void WriteLine(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                LogMessages.Enqueue(message);
            }
        }

        public static void FlushWith(ILogger logger)
        {
            while (LogMessages.TryDequeue(out string message))
            {
                logger.Information("{Message}", message);
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }
}