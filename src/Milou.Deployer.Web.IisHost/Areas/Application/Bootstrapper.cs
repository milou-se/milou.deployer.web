using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class Bootstrapper
    {
        public static void Start(
            [NotNull] IReadOnlyList<IModule> modulesToRegister,
            IServiceCollection serviceCollection,
            [NotNull] ILogger logger)
        {
            foreach (var module in modulesToRegister)
            {
                if (logger.IsEnabled(LogEventLevel.Verbose))
                {
                    var type = module.GetType();

                    logger.Verbose("Registering pre-initialized module {Module} in container builder",
                        $"{type.FullName} assembly {type.Assembly.FullName} at {type.Assembly.Location}");
                }

                serviceCollection = module.Register(serviceCollection);
            }

            if (logger.IsEnabled(LogEventLevel.Debug))
            {
                logger.Debug("Done running configuration modules");
            }
        }
    }
}
