using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.EventSource;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace Milou.Deployer.Web.Agent
{
    public class DeploymentTaskLogController : BaseApiController
    {
        private readonly ILogger _logger;

        public DeploymentTaskLogController(ILogger logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [Route("/deployment-task/log")]
        public async Task<IActionResult> Log([FromBody] SerilogSinkEvents events, [FromServices] IMediator mediator)
        {
            string deploymentTaskId = Request.Headers["x-deployment-task-id"];
            string deploymentTargetId = Request.Headers["x-deployment-target-id"];

            if (events?.Events is null)
            {
                return Ok();
            }

            foreach (var serilogSinkEvent in events.Events)
            {
                if (serilogSinkEvent is null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(deploymentTaskId) && !string.IsNullOrWhiteSpace(deploymentTargetId) && !string.IsNullOrWhiteSpace(serilogSinkEvent.RenderedMessage))
                {
                    await mediator.Publish(new AgentLogNotification(deploymentTaskId, deploymentTargetId,
                        serilogSinkEvent.RenderedMessage));
                }

                //try
                //{
                //    IEnumerable<LogEventProperty> properties = serilogSinkEvent.Properties?.Select(property =>
                //        new LogEventProperty(property.Key, new ScalarValue(property.Value.ToString()))) ;
                //    var logEvent = new LogEvent(serilogSinkEvent.Timestamp, serilogSinkEvent.Level, null,
                //        new MessageTemplate(serilogSinkEvent.MessageTemplate,
                //            ImmutableArray<MessageTemplateToken>.Empty), properties?? ImmutableArray<LogEventProperty>.Empty);
                //    //_logger.Write(logEvent);
                //}
                //catch (Exception ex)
                //{
                //    _logger.Error(ex, "Could not log event {Event}", serilogSinkEvent);
                //}
            }

            return Ok();
        }
    }

    public class SerilogSinkEvents{

        public SerilogSinkEvent[] Events { get; set; }
    }

    public class SerilogSinkEvent
    {
      public DateTimeOffset Timestamp { get; set; }
      public LogEventLevel Level { get; set; }
      public string MessageTemplate { get; set; }
      public string RenderedMessage { get; set; }
      public Dictionary<string, object> Properties { get; set; }
    }
}