using Arbor.App.Extensions.DependencyInjection;
using Arbor.AspNetCore.Host.Mvc;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class MvcViewsModule : IModule
    {
        private readonly ILogger _logger;

        public MvcViewsModule(ILogger logger) => _logger = logger;

        public IServiceCollection Register(IServiceCollection builder)
        {
            ViewAssemblyLoader.LoadViewAssemblies(_logger);

            return builder;
        }
    }
}