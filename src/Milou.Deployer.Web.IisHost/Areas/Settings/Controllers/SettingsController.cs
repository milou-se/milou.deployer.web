using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.IisHost.Controllers;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    [Area(SettingsConstants.AreaName)]
    public class SettingsController : BaseApiController
    {
        public const string BaseRoute = "settings";

        [HttpGet]
        [Route(SettingsConstants.SettingsGetRoute, Name = SettingsConstants.SettingsGetRouteName)]
        public async Task<ActionResult<SettingsViewModel>> Index(
            [FromServices] MultiSourceKeyValueConfiguration configuration,
            [FromServices] IMediator mediator)
        {
            if (!configuration[SettingsConstants.DiagnosticsEnabled].ParseAsBooleanOrDefault())
            {
                return new StatusCodeResult(403);
            }

            SettingsViewModel settingsViewModel = await mediator.Send(new SettingsViewRequest());

            return View(settingsViewModel);
        }

        [HttpPost]
        [Route(SettingsConstants.LogSettingsPostRoute, Name = SettingsConstants.LogSettingsPostRouteName)]
        public async Task<IActionResult> LogLevel(
            [FromBody] ChangeLogLevel changeLogLevel,
            [FromServices] IMediator mediator)
        {
            await mediator.Send(new ChangeLogLevelRequest(changeLogLevel));

            return RedirectToAction(nameof(Index));
        }
    }

    public class ChangeLogLevelRequest : IRequest
    {
        public ChangeLogLevelRequest(ChangeLogLevel changeLogLevel)
        {
            ChangeLogLevel = changeLogLevel;
        }

        public ChangeLogLevel ChangeLogLevel { get; }
    }

    [UsedImplicitly]
    public class ChangeLogLevelRequestHandler : IRequestHandler<ChangeLogLevelRequest>
    {
        private static readonly ImmutableDictionary<string, LogEventLevel> _Levels =
            Enum.GetNames(typeof(LogEventLevel))
                .Select(name =>
                    (name, Enum.TryParse(name, out LogEventLevel foundLevel), foundLevel))
                .Where(level => level.Item2)
                .ToImmutableDictionary(level => level.name,
                    level => level.foundLevel,
                    StringComparer.OrdinalIgnoreCase);

        private readonly LoggingLevelSwitch _levelSwitch;
        private readonly ILogger _logger;

        public ChangeLogLevelRequestHandler(LoggingLevelSwitch levelSwitch, ILogger logger)
        {
            _levelSwitch = levelSwitch;
            _logger = logger;
        }

        public Task<Unit> Handle(ChangeLogLevelRequest request, CancellationToken cancellationToken)
        {
            if (TryParse(request.ChangeLogLevel.NewLevel, out LogEventLevel newLevel))
            {
                LogEventLevel oldLevel = _levelSwitch.MinimumLevel;

                if (oldLevel != newLevel)
                {
                    _logger.Information("Switching log level from {OldLogLevel} to {NewLogLevel}",
                        oldLevel,
                        newLevel);

                    _levelSwitch.MinimumLevel = newLevel;

                    _logger.Information("Switched log level from {OldLogLevel} to {NewLogLevel}",
                        oldLevel,
                        newLevel);
                }
            }

            return Unit.Task;
        }

        private static bool TryParse(string attemptedValue, out LogEventLevel logEventLevel)
        {
            if (string.IsNullOrWhiteSpace(attemptedValue))
            {
                logEventLevel = default;
                return false;
            }

            return _Levels.TryGetValue(attemptedValue, out logEventLevel);
        }
    }

    public class ChangeLogLevel
    {
        public ChangeLogLevel(string newLevel)
        {
            NewLevel = newLevel;
        }

        public string NewLevel { get; }
    }
}