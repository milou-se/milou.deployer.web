using System;
using System.Collections.Generic;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Agents
{
    public class SerilogSinkEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public LogEventLevel Level { get; set; }
        public string MessageTemplate { get; set; }
        public string RenderedMessage { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}