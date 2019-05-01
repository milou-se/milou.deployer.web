﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.Core.Logging
{
    [UsedImplicitly]
    public class ChangeLogLevelRequestHandler : IRequestHandler<ChangeLogLevelRequest>
    {
        private static readonly ImmutableDictionary<string, LogEventLevel> Levels =
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

        private static bool TryParse(string attemptedValue, out LogEventLevel logEventLevel)
        {
            if (string.IsNullOrWhiteSpace(attemptedValue))
            {
                logEventLevel = default;
                return false;
            }

            return Levels.TryGetValue(attemptedValue, out logEventLevel);
        }

        public Task<Unit> Handle(ChangeLogLevelRequest request, CancellationToken cancellationToken)
        {
            if (TryParse(request.ChangeLogLevel.NewLevel, out var newLevel))
            {
                var oldLevel = _levelSwitch.MinimumLevel;

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
    }
}
