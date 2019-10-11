using System;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    public class DelegateSink : ILogEventSink
    {
        private readonly Action<string, LogEventLevel> _action;

        public DelegateSink([NotNull] Action<string, LogEventLevel> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Emit([NotNull] LogEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            var renderedTemplate = logEvent.MessageTemplate.Render(logEvent.Properties);

            var output = new
            {
                logEvent.MessageTemplate,
                logEvent.Properties,
                logEvent.Exception,
                Level = logEvent.Level.ToString(),
                logEvent.Timestamp,
                RenderedTemplate = renderedTemplate,
                FormattedTimestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
            };

            var message = JsonConvert.SerializeObject(output);

            _action(message, logEvent.Level);
        }
    }
}
