using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
using Milou.Deployer.Web.Core;
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
            [NotNull] ILogger appLogger,
            MultiSourceKeyValueConfiguration configuration)
        {
            CancellationTokenSource = cancellationTokenSource ??
                                      throw new ArgumentNullException(nameof(cancellationTokenSource));
            Logger = appLogger ?? throw new ArgumentNullException(nameof(appLogger));
            Configuration = configuration;
            HostBuilder = webHost ?? throw new ArgumentNullException(nameof(webHost));
        }

        public CancellationTokenSource CancellationTokenSource { get; }

        public ILogger Logger { get; private set; }

        public MultiSourceKeyValueConfiguration Configuration { get; private set; }

        [PublicAPI]
        public IWebHostBuilder HostBuilder { get; private set; }

        public IWebHost WebHost { get; private set; }

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

            Logger?.Debug("Disposing application");
            Logger?.Verbose("Disposing web host");
            WebHost?.SafeDispose();
            Logger?.Verbose("Disposing Application root scope");
            Scope rootScope = AppRootScope.Top();
            AppRootScope?.SafeDispose();
            rootScope?.SafeDispose();
            Logger?.Verbose("Disposing configuration");
            Configuration?.SafeDispose();

            Logger?.Debug("Application disposal complete, disposing logging");

            if (Logger is IDisposable disposable)
            {
                Logger?.Verbose("Disposing Logger");
                disposable.SafeDispose();
            }
            else
            {
                Logger?.Debug("Logger is not disposable");
            }

            Configuration = null;
            Logger = null;
            WebHost = null;
            HostBuilder = null;
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

            try
            {
                WebHost = HostBuilder.Build();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                Logger.Fatal(ex, "Could not build web host");
                throw new DeployerAppException("Could not build web host", ex);
            }

            if (args.Any(arg => arg.Equals(ApplicationConstants.RunAsService)))
            {
                Logger.Information("Starting as a Windows Service");

                try
                {
                    WebHost.CustomRunAsService(this);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    Logger.Fatal(ex, "Could not start web host as a Windows service");
                    throw new DeployerAppException($"Could not start web host as a Windows service, configuration {Configuration?.SourceChain}", ex);
                }
            }
            else
            {
                Logger.Information("Starting as a Console Application");

                try
                {
                    await WebHost.StartAsync(CancellationTokenSource.Token);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    Logger.Fatal(ex, "Could not start web host");
                    throw new DeployerAppException($"Could not start web host, configuration {Configuration?.SourceChain}",ex);
                }
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

            bool IsRunningAsService()
            {
                bool fromArg = args.Any(arg =>
                    arg.Equals(ApplicationConstants.RunAsService, StringComparison.OrdinalIgnoreCase));

                if (fromArg)
                {
                    return true;
                }

                FileInfo processFileInfo;
                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    processFileInfo = new FileInfo(currentProcess.MainModule.FileName);
                }

                if (processFileInfo.Name.Equals("Milou.Deployer.Web.IisHost.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            if (IsRunningAsService())
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }

            string basePath = basePathFromArg ?? AppDomain.CurrentDomain.BaseDirectory;
            string contentBasePath = contentBasePathFromArg ?? Directory.GetCurrentDirectory();

            ILogger startupLogger =
                SerilogApiInitialization.InitializeStartupLogging(file => GetBaseDirectoryFile(basePath, file));

            startupLogger.Information("Using application root directory {Directory}", basePath);

            MultiSourceKeyValueConfiguration configuration =
                ConfigurationInitialization.InitializeConfiguration(args,
                    file => GetBaseDirectoryFile(basePath, file),
                    startupLogger, scanAssemblies, contentBasePath);

            string tempDirectory = configuration[ApplicationConstants.ApplicationTempDirectory];

            if (!string.IsNullOrWhiteSpace(tempDirectory))
            {
                if (tempDirectory.TryEnsureDirectoryExists(out DirectoryInfo tempDirectoryInfo))
                {
                    Environment.SetEnvironmentVariable(TempConstants.Tmp, tempDirectoryInfo.FullName);
                    Environment.SetEnvironmentVariable(TempConstants.Temp, tempDirectoryInfo.FullName);

                    startupLogger.Debug("Using specified temp directory {TempDirectory}", tempDirectory);
                }
                else
                {
                    startupLogger.Warning("Could not use specified temp directory {TempDirectory}", tempDirectory);
                }
            }

            var loggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug); // TODO make configurable

            ILogger appLogger =
                SerilogApiInitialization.InitializeAppLogging(configuration, startupLogger, loggerConfigurationAction, loggingLevelSwitch);

            if (args.Length > 0)
            {
                appLogger.Debug("Application started with command line args, {Args}", args);
            }
            else if (appLogger.IsEnabled(LogEventLevel.Verbose))
            {
                appLogger.Verbose("Application started with no command line args");
            }

            IReadOnlyList<IModule> modules =
                GetConfigurationModules(configuration, cancellationTokenSource, appLogger, scanAssemblies);

            Type[] excludedModuleTypes = { typeof(AppServiceModule) };

            var environmentConfiguration = new EnvironmentConfiguration
            {
                ApplicationBasePath = basePath,
                ContentBasePath = contentBasePath
            };

            var singletons = new object[] { loggingLevelSwitch, environmentConfiguration};

            Scope rootScope = Bootstrapper.Start(configuration,
                modules, appLogger, scanAssemblies, excludedModuleTypes,singletons);

            DeploymentTargetIds deploymentTargetIds = await GetDeploymentWorkerIdsAsync(rootScope.Deepest().Lifetime, appLogger, cancellationTokenSource.Token);

            ILifetimeScope webHostScope =
                rootScope.Deepest().Lifetime.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance(deploymentTargetIds).AsSelf().SingleInstance();
                    builder.RegisterType<Startup>().AsSelf();
                });

            var webHostScopeWrapper = new Scope(Scope.WebHostScope, webHostScope);
            rootScope.Deepest().SubScope = webHostScopeWrapper;

            EnvironmentConfigurator.ConfigureEnvironment(rootScope.Deepest().Lifetime);

            IWebHostBuilder webHostBuilder =
                CustomWebHostBuilder.GetWebHostBuilder(configuration, rootScope, webHostScopeWrapper, appLogger, rootScope.Top());

            var app = new App(webHostBuilder, cancellationTokenSource, appLogger, configuration)
            {
                AppRootScope = rootScope.SubScope
            };

            return app;
        }

        private static async Task<DeploymentTargetIds> GetDeploymentWorkerIdsAsync(
            ILifetimeScope scope,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var dataSeeders = scope.Resolve<IReadOnlyCollection<IDataSeeder>>();

            foreach (IDataSeeder dataSeeder in dataSeeders)
            {
                await dataSeeder.SeedAsync(cancellationToken);
            }

            try
            {

                scope.TryResolve(out IDeploymentTargetReadService deploymentTargetReadService);

                IReadOnlyCollection<string> targetIds = deploymentTargetReadService != null
                    ? (await deploymentTargetReadService.GetDeploymentTargetsAsync(default))
                    .Select(deploymentTarget => deploymentTarget.Id)
                    .ToArray()
                    : Array.Empty<string>();

                return new DeploymentTargetIds(targetIds);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                logger.Warning(ex, "Could not get target ids");
                return new DeploymentTargetIds(Array.Empty<string>());
            }
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
            modules.Add(new DataModule(logger));

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
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return basePath;
            }

            return Path.Combine(basePath, fileName);
        }
    }
}