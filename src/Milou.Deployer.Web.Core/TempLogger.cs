using System;
using System.Collections.Concurrent;
using Serilog;

namespace Milou.Deployer.Web.Core
{
    public static class TempLogger
    {
        private static readonly ConcurrentQueue<string> _logMessages = new ConcurrentQueue<string>();

        public static void WriteLine(string message)
        {
            _logMessages.Enqueue(message);

            Console.WriteLine(message);
        }

        public static void FlushWith(ILogger logger)
        {
            while (_logMessages.TryDequeue(out string message))
            {
                logger.Information("{Message}", message);
            }
        }
    }
}