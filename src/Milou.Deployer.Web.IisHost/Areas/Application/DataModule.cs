using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Targets;
using Serilog;
using Module = Autofac.Module;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class DataModule : Module
    {
        private readonly ILogger _logger;

        public DataModule([NotNull] ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void Load(ContainerBuilder builder)
        {
            Assembly[] scanAssemblies = Assemblies.FilteredAssemblies().ToArray();

            builder.RegisterAssemblyTypes(scanAssemblies)
                .Where(type => type.IsConcreteTypeImplementing<IDataSeeder>())
                .AsImplementedInterfaces()
                .OnRegistered(OnRegistered);
        }

        private void OnRegistered(ComponentRegisteredEventArgs arg)
        {
            _logger.Verbose("Registered data seed {SeedServiceDescription}",
                arg.ComponentRegistration.Services.Select(s => s.Description).ToArray());
        }
    }
}