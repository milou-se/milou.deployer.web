using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.Core.Targets;
using Milou.Deployer.Web.IisHost.Areas.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Configuration.Modules;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Messaging;
using Milou.Deployer.Web.IisHost.AspNetCore;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using KeyValueConfigurationModule = Milou.Deployer.Web.IisHost.Areas.Configuration.KeyValueConfigurationModule;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public sealed class App : IDisposable
    {
        private bool _disposed;
        private bool _disposing;

        public App(
            [NotNull] IWebHostBuilder webHost,
            [NotNull] CancellationTokenSource cancellationTokenSource,
            [NotNull] ILogger appLogger)
        {
            CancellationTokenSource = cancellationTokenSource ??
                                      throw new ArgumentNullException(nameof(cancellationTokenSource));
            Logger = appLogger ?? throw new ArgumentNullException(nameof(appLogger));
            HostBuilder = webHost ?? throw new ArgumentNullException(nameof(webHost));
        }

        public CancellationTokenSource CancellationTokenSource { get; }

        public ILogger Logger { get; private set; }

        [PublicAPI]
        public IWebHostBuilder HostBuilder { get; private set; }

        public IWebHost WebHost { get; private set; }

        public IContainer Container { get; private set; }

        public Scope AppRootScope { get; private set; }

        public static async Task<App> CreateAsync(
            CancellationTokenSource cancellationTokenSource,
            Action<LoggerConfiguration> loggerConfigurationAction,
            params string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            try
            {
                App app = await BuildAppAsync(cancellationTokenSource, loggerConfigurationAction, args);

                return app;
            }
            catch (Exception ex)
            catch (Exception ex) when (!ex.IsFatal())
            {
                Console.Error.WriteLine("Error in startup, " + ex);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed || _disposing)
            {
                return;
            }

            if (!_disposing)
            {
                Stop();
                _disposing = true;
            }

            Logger?.Verbose("Disposing application");
            Logger?.Verbose("Disposing web host");
            WebHost?.Dispose();
            Logger?.Verbose("Disposing Application root scope");
            AppRootScope?.Dispose();
            Logger?.Verbose("Disposing container");
            Container?.Dispose();

            if (Logger is IDisposable disposable)
            {
                Logger?.Verbose("Disposing Logger");
                disposable.Dispose();
            }

            Logger = null;
            WebHost = null;
            HostBuilder = null;
            Container = null;
            AppRootScope = null;
            _disposed = true;
            _disposing = false;
        }

        [PublicAPI]
        public void Stop()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }
        }

        public async Task<int> RunAsync([NotNull] params string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            WebHost = HostBuilder.Build();

            if (args.Any(arg => arg.Equals(ApplicationConstants.RunAsService)))
            {
                Logger.Information("Starting as a Windows Service");
                WebHost.RunAsService();
            }
            else
            {
                Logger.Information("Starting as a Console Application");
                await WebHost.StartAsync(CancellationTokenSource.Token);
            }

            return 0;
        }

        private static async Task<App> BuildAppAsync(
            CancellationTokenSource cancellationTokenSource,
            Action<LoggerConfiguration> loggerConfigurationAction,
            string[] args)
        {
            ImmutableArray<Assembly> scanAssemblies = Assemblies.FilteredAssemblies();

            string basePathFromArg = args.ParseParameter(ConfigurationConstants.ApplicationBasePath);

            string contentBasePathFromArg = args.ParseParameter(ConfigurationConstants.ContentBasePath);

            string basePath = basePathFromArg ?? AppDomain.CurrentDomain.BaseDirectory;
            string contentBasePath = contentBasePathFromArg ?? Directory.GetCurrentDirectory();

            ILogger startupLogger =
                SerilogApiInitialization.InitializeStartupLogging(file => GetBaseDirectoryFile(basePath, file));

            startupLogger.Information("Using application root directory {Directory}", basePath);

            MultiSourceKeyValueConfiguration configuration =
                ConfigurationInitialization.InitializeConfiguration(args,
                    file => GetBaseDirectoryFile(basePath, file),
                    startupLogger, scanAssemblies, contentBasePath);

            var loggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug); // TODO make configurable

            ILogger appLogger =
                SerilogApiInitialization.InitializeAppLogging(configuration, startupLogger, loggerConfigurationAction, loggingLevelSwitch);

            appLogger.Debug("Started with command line args, {Args}", args);

            IReadOnlyList<IModule> modules =
                GetConfigurationModules(configuration, cancellationTokenSource, appLogger, scanAssemblies);

            Type[] excludedModuleTypes = { typeof(AppServiceModule) };

            AppContainerScope container = Bootstrapper.Start(basePath, contentBasePath, modules, appLogger, scanAssemblies, excludedModuleTypes, loggingLevelSwitch);

            DeploymentTargetIds deploymentTargetIds = await GetDeploymentWorkerIdsAsync(container.AppRootScope.Deepest().Lifetime, cancellationTokenSource.Token);

            ILifetimeScope webHostScope =
                container.AppRootScope.Deepest().Lifetime.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance(deploymentTargetIds).AsSelf().SingleInstance();
                    builder.RegisterType<Startup>().AsSelf();
                    builder.RegisterInstance(container.AppRootScope);
                });

            var webHostScopeWrapper = new Scope(webHostScope);
            container.AppRootScope.Deepest().SubScope = webHostScopeWrapper;

            IWebHostBuilder webHostBuilder =
                CustomWebHostBuilder.GetWebHostBuilder(configuration, container.AppRootScope, webHostScopeWrapper, appLogger);

            var app = new App(webHostBuilder, cancellationTokenSource, appLogger)
            {
                Container = container.Container,
                AppRootScope = container.AppRootScope
            };

            return app;
        }

        private static async Task<DeploymentTargetIds> GetDeploymentWorkerIdsAsync(ILifetimeScope scope, CancellationToken cancellationToken)
        {
            var dataSeeders = scope.Resolve<IReadOnlyCollection<IDataSeeder>>();

            foreach (IDataSeeder dataSeeder in dataSeeders)
            {
                await dataSeeder.SeedAsync(cancellationToken);
            }

            var deploymentTargetReadService = scope.Resolve<IDeploymentTargetReadService>();

            IReadOnlyCollection<string> targetIds =
                (await deploymentTargetReadService.GetDeploymentTargetsAsync(default))
                .Select(deploymentTarget => deploymentTarget.Id)
                .ToArray();

            return new DeploymentTargetIds(targetIds);
        }

        private static IReadOnlyList<IModule> GetConfigurationModules(
            MultiSourceKeyValueConfiguration configuration,
            CancellationTokenSource cancellationTokenSource,
            ILogger logger,
            ImmutableArray<Assembly> scanAssemblies)
        {
            var modules = new List<IModule>();

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            var loggingModule = new LoggingModule(logger);

            var module = new KeyValueConfigurationModule(configuration, logger);

            var urnModule = new UrnConfigurationModule(configuration, logger, scanAssemblies);

            Type[] excludedTypes = configuration.GetInstances<ExcludedAutoRegistrationType>()
                .Select(TryGetType)
                .Where(type => type != null)
                .ToArray();

            modules.Add(loggingModule);
            modules.Add(module);
            modules.Add(urnModule);
            modules.Add(new MediatorModule(scanAssemblies, excludedTypes, logger));

            return modules;
        }

        private static Type TryGetType(ExcludedAutoRegistrationType excluded)
        {
            try
            {
                return Type.GetType(excluded.FullName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetBaseDirectoryFile(string basePath, string fileName)
        {
            return Path.Combine(basePath, fileName);
        }
    }
}