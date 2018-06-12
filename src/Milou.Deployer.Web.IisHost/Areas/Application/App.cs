using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Configuration.Modules;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Messaging;
using Milou.Deployer.Web.IisHost.AspNetCore;
using Serilog;
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

        public ILogger Logger { get; }

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
            {
                Console.Error.WriteLine("Error in startup, " + ex);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (!_disposing)
            {
                Stop();
                _disposing = true;
            }

            WebHost?.Dispose();
            AppRootScope?.Dispose();
            Container?.Dispose();

            if (Logger is IDisposable disposable)
            {
                disposable.Dispose();
            }

            WebHost = null;
            HostBuilder = null;
            Container = null;
            AppRootScope = null;
            _disposed = true;
            _disposing = false;
        }

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

            await WebHost.StartAsync(CancellationTokenSource.Token);

            return 0;
        }

        private static async Task<App> BuildAppAsync(
            CancellationTokenSource cancellationTokenSource,
            Action<LoggerConfiguration> loggerConfigurationAction,
            string[] args)
        {
            ImmutableArray<Assembly> scanAssemblies = AppDomain.CurrentDomain.FilteredAssemblies();

            string basePathFromArg = args.SingleOrDefault(arg =>
                arg.StartsWith(ConfigurationConstants.BasePath, StringComparison.OrdinalIgnoreCase));

            string basePath = basePathFromArg?.Split('=').LastOrDefault() ?? AppDomain.CurrentDomain.BaseDirectory;

            ILogger startupLogger =
                SerilogApiInitialization.InitializeStartupLogging(file => GetBaseDirectoryFile(basePath, file));

            startupLogger.Information("Using application root directory {Directory}", basePath);

            MultiSourceKeyValueConfiguration configuration =
                ConfigurationInitialization.InitializeConfiguration(args,
                    file => GetBaseDirectoryFile(basePath, file),
                    startupLogger, scanAssemblies);

            ILogger appLogger =
                SerilogApiInitialization.InitializeAppLogging(configuration, startupLogger, loggerConfigurationAction);

            appLogger.Debug("Started with command line args, {Args}", args);

            IReadOnlyList<IModule> modules =
                GetConfigurationModules(configuration, cancellationTokenSource, appLogger, scanAssemblies);

            Type[] excluded = { typeof(AppServiceModule) };

            AppContainerScope container = Bootstrapper.Start(basePath, modules, appLogger, scanAssemblies, excluded);

            var appRootScope = new Scope(container.AppRootScope);

            DeploymentTargetIds deploymentTargetIds = await GetDeploymentWorkerIdsAsync(container.AppRootScope);

            ILifetimeScope webHostScope =
                container.AppRootScope.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance(deploymentTargetIds).AsSelf().SingleInstance();
                    builder.RegisterType<Startup>().AsSelf();
                    builder.RegisterInstance(appRootScope);
                });

            var webHostScopeWrapper = new Scope(webHostScope);
            appRootScope.SubScope = webHostScopeWrapper;

            IWebHostBuilder webHostBuilder =
                CustomWebHostBuilder.GetWebHostBuilder(appRootScope, webHostScopeWrapper, appLogger);

            var app = new App(webHostBuilder, cancellationTokenSource, appLogger)
            {
                Container = container.Container,
                AppRootScope = appRootScope
            };

            return app;
        }

        private static async Task<DeploymentTargetIds> GetDeploymentWorkerIdsAsync(ILifetimeScope scope)
        {
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

            modules.Add(loggingModule);
            modules.Add(module);
            modules.Add(urnModule);
            modules.Add(new MediatRModule(scanAssemblies));

            return modules;
        }

        private static string GetBaseDirectoryFile(string basePath, string fileName)
        {
            return Path.Combine(basePath, fileName);
        }
    }
}