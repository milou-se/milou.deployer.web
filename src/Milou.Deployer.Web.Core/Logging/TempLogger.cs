using System;
using System.Collections.Concurrent;
using Serilog;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class TempLogger
    {
        private static readonly ConcurrentQueue<string> LogMessages = new ConcurrentQueue<string>();

        public static void WriteLine(string message)
        {
            LogMessages.Enqueue(message);

            Console.WriteLine(message);
        }

        public static void FlushWith(ILogger logger)
        {
            while (LogMessages.TryDequeue(out var message))
            {
                logger.Information("{Message}", message);
            }
        }
    }
}
