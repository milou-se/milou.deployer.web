using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.DependencyInjection;
using Serilog;

namespace Milou.Deployer.Web.Core.Logging
{
    [UsedImplicitly]
    public class LoggingModule : IModule
    {
        private readonly ILogger _logger;

        public LoggingModule([NotNull] ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.AddSingleton(_logger, this);
        }
    }
}
