using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Arbor.Tooler;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.Core.NuGet;
using Milou.Deployer.Web.Core.Targets;
using Milou.Deployer.Web.IisHost.Areas.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Configuration.Modules;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Messaging;
using Milou.Deployer.Web.IisHost.AspNetCore;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public sealed class App : IDisposable
    {
        private readonly Guid _instanceId;
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
            _instanceId = Guid.NewGuid();
            AppInstance = ApplicationConstants.ApplicationName + " " + _instanceId;
        }

        public string AppInstance { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        public ILogger Logger { get; private set; }

        public MultiSourceKeyValueConfiguration Configuration { get; private set; }

        [PublicAPI]
        public IWebHostBuilder HostBuilder { get; private set; }

        public IWebHost WebHost { get; private set; }

        public Scope AppRootScope { get; private set; }

        private void LogConfigurationValues()
        {
            var configurationValues = AppRootScope.Deepest().GetConfigurationValues();
            Logger.Debug("Using configuration values {ConfigurationValues}",
                string.Join(Environment.NewLine,
                    configurationValues.Select(configurationValue => configurationValue.Item2)));
        }

        private static async Task<App> BuildAppAsync(
            CancellationTokenSource cancellationTokenSource,
            Action<LoggerConfiguration> loggerConfigurationAction,
            string[] commandLineArgs,
            ImmutableDictionary<string, string> environmentVariables)
        {
            var scanAssemblies = Assemblies.FilteredAssemblies();

            var basePathFromArg = commandLineArgs.ParseParameter(ConfigurationConstants.ApplicationBasePath);

            var contentBasePathFromArg = commandLineArgs.ParseParameter(ConfigurationConstants.ContentBasePath);

            bool IsRunningAsService()
            {
                var hasRunAsServiceArgument = commandLineArgs.Any(arg =>
                    arg.Equals(ApplicationConstants.RunAsService, StringComparison.OrdinalIgnoreCase));

                if (hasRunAsServiceArgument)
                {
                    return true;
                }

                FileInfo processFileInfo;
                using (var currentProcess = Process.GetCurrentProcess())
                {
                    processFileInfo = new FileInfo(currentProcess.MainModule.FileName);
                }

                if (processFileInfo.Name.Equals("Milou.Deployer.Web.WindowsService.exe",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            var currentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (IsRunningAsService())
            {
                TempLogger.WriteLine(
                    $"Switching current directory from {Directory.GetCurrentDirectory()} to {currentDomainBaseDirectory}");
                Directory.SetCurrentDirectory(currentDomainBaseDirectory);
            }

            var basePath = basePathFromArg ?? currentDomainBaseDirectory;
            var contentBasePath = contentBasePathFromArg ?? Directory.GetCurrentDirectory();

            var startupLogger =
                SerilogApiInitialization.InitializeStartupLogging(file => GetBaseDirectoryFile(basePath, file),
                    environmentVariables);

            startupLogger.Information("Using application root directory {Directory}", basePath);

            MultiSourceKeyValueConfiguration configuration;
            try
            {
                configuration =
                    ConfigurationInitialization.InitializeConfiguration(file => GetBaseDirectoryFile(basePath, file),
                        contentBasePath,
                        scanAssemblies,
                        commandLineArgs,
                        environmentVariables);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                startupLogger.Fatal(ex, "Could not initialize configuration");
                throw;
            }

            startupLogger.Information("Configuration done using chain {Chain}",
                configuration.SourceChain);

            startupLogger.Verbose("Configuration values {KeyValues}",
                configuration.AllValues.Select(pair =>
                    $"\"{pair.Key}\": \"{pair.Value.MakeAnonymous(pair.Key, $"{StringExtensions.DefaultAnonymousKeyWords.ToArray()}\"")}"));

            var tempDirectory = configuration[ApplicationConstants.ApplicationTempDirectory];

            if (!string.IsNullOrWhiteSpace(tempDirectory))
            {
                if (tempDirectory.TryEnsureDirectoryExists(out var tempDirectoryInfo))
                {
                    Environment.SetEnvironmentVariable(TempConstants.Tmp, tempDirectoryInfo.FullName);
                    Environment.SetEnvironmentVariable(TempConstants.Temp, tempDirectoryInfo.FullName);

                    startupLogger.Debug("Using specified temp directory {TempDirectory} {AppName}",
                        tempDirectory,
                        ApplicationConstants.ApplicationName);
                }
                else
                {
                    startupLogger.Warning("Could not use specified temp directory {TempDirectory}, {AppName}",
                        tempDirectory,
                        ApplicationConstants.ApplicationName);
                }
            }

            var defaultLevel = configuration[ConfigurationConstants.LogLevel]
                .ParseOrDefault(LogEventLevel.Information);

            var loggingLevelSwitch = new LoggingLevelSwitch(defaultLevel);

            ILogger appLogger;
            try
            {
                appLogger =
                    SerilogApiInitialization.InitializeAppLogging(
                        configuration,
                        startupLogger,
                        loggerConfigurationAction,
                        loggingLevelSwitch);
            }
            catch (Exception ex)
            {
                appLogger = startupLogger;
                startupLogger.Error(ex, "Could not create app logger");
            }

            if (commandLineArgs.Length > 0)
            {
                appLogger.Debug("Application started with command line args, {Args}, {AppName}",
                    commandLineArgs,
                    ApplicationConstants.ApplicationName);
            }
            else if (appLogger.IsEnabled(LogEventLevel.Verbose))
            {
                appLogger.Verbose("Application started with no command line args, {AppName}",
                    ApplicationConstants.ApplicationName);
            }

            var modules =
                GetConfigurationModules(configuration, cancellationTokenSource, appLogger, scanAssemblies);

            Type[] excludedModuleTypes = { typeof(AppServiceModule) };

            var environmentConfiguration = new EnvironmentConfiguration
            {
                ApplicationBasePath = basePath,
                ContentBasePath = contentBasePath,
                CommandLineArgs = commandLineArgs.ToImmutableArray(),
                EnvironmentName = environmentVariables.ValueOrDefault(ApplicationConstants.AspNetEnvironment)
            };

            var singletons = new object[] { loggingLevelSwitch, environmentConfiguration, new NuGetConfiguration()};

            Scope rootScope;

            try
            {
                rootScope = Bootstrapper.Start(
                    configuration,
                    modules,
                    appLogger,
                    scanAssemblies,
                    excludedModuleTypes,
                    singletons);
            }
            catch (Exception ex)
            {
                appLogger.Fatal(ex, "Could not initialize container registrations");
                throw;
            }

            var deploymentTargetIds = await GetDeploymentWorkerIdsAsync(rootScope.Deepest().Lifetime,
                appLogger,
                configuration,
                cancellationTokenSource.Token);

            var nugetExePath = "";

            appLogger.Debug("Ensuring nuget.exe exists");

            if (!int.TryParse(configuration[ConfigurationConstants.NuGetDownloadTimeoutInSeconds],
                    out var initialNuGetDownloadTimeoutInSeconds) || initialNuGetDownloadTimeoutInSeconds <= 0)
            {
                initialNuGetDownloadTimeoutInSeconds = 100;
            }

            try
            {
                using (var cts =
                    new CancellationTokenSource(TimeSpan.FromSeconds(initialNuGetDownloadTimeoutInSeconds)))
                {
                    var downloadDirectory = configuration[ConfigurationConstants.NuGetExeDirectory].WithDefault(null);

                    using (var httpClient = new HttpClient())
                    {
                        var nuGetDownloadResult = await new NuGetDownloadClient().DownloadNuGetAsync(
                            new NuGetDownloadSettings(downloadDirectory: downloadDirectory),
                            appLogger,
                            httpClient,
                            cts.Token);

                        if (nuGetDownloadResult.Succeeded)
                        {
                            nugetExePath = nuGetDownloadResult.NuGetExePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                appLogger.Warning(ex, "Could not download nuget.exe");
            }

            var nugetConfiguration = rootScope.Deepest().Lifetime.Resolve<NuGetConfiguration>();

            nugetConfiguration.NugetExePath = nugetExePath;

            var webHostScope =
                rootScope.Deepest().Lifetime.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance(deploymentTargetIds).AsSelf().SingleInstance();
                    builder.RegisterType<Startup>().AsSelf();
                    builder.RegisterInstance(nugetConfiguration).AsSelf();
                });

            var webHostScopeWrapper = new Scope(Scope.WebHostScope, webHostScope);
            rootScope.Deepest().SubScope = webHostScopeWrapper;

            EnvironmentConfigurator.ConfigureEnvironment(rootScope.Deepest().Lifetime);

            var webHostBuilder =
                CustomWebHostBuilder.GetWebHostBuilder(configuration,
                    rootScope,
                    webHostScopeWrapper,
                    appLogger,
                    rootScope.Top());

            var app = new App(webHostBuilder, cancellationTokenSource, appLogger, configuration)
            {
                AppRootScope = rootScope.SubScope
            };

            return app;
        }

        private static async Task<DeploymentTargetIds> GetDeploymentWorkerIdsAsync(
            ILifetimeScope scope,
            ILogger logger,
            MultiSourceKeyValueConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var dataSeeders = scope.Resolve<IReadOnlyCollection<IDataSeeder>>();

            if (dataSeeders.Count > 0)
            {
                if (!int.TryParse(configuration[ConfigurationConstants.SeedTimeoutInSeconds],
                        out var seedTimeoutInSeconds) ||
                    seedTimeoutInSeconds <= 0)
                {
                    seedTimeoutInSeconds = 10;
                }

                logger.Debug("Running data seeders");

                foreach (var dataSeeder in dataSeeders)
                {
                    using (var startupToken = new CancellationTokenSource(TimeSpan.FromSeconds(seedTimeoutInSeconds)))
                    {
                        using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                            cancellationToken,
                            startupToken.Token))
                        {
                            logger.Debug("Running data seeder {Seeder}", dataSeeder.GetType().FullName);
                            await dataSeeder.SeedAsync(linkedToken.Token);
                        }
                    }
                }

                logger.Debug("Done running data seeders");
            }
            else
            {
                logger.Debug("No data seeders were found");
            }

            try
            {
                if (!int.TryParse(configuration[ConfigurationConstants.StartupTargetsTimeoutInSeconds],
                        out var startupTimeoutInSeconds) ||
                    startupTimeoutInSeconds <= 0)
                {
                    startupTimeoutInSeconds = 10;
                }

                using (var startupToken = new CancellationTokenSource(TimeSpan.FromSeconds(startupTimeoutInSeconds)))
                {
                    using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                        startupToken.Token))
                    {
                        var deploymentTargetReadService = scope.ResolveOptional<IDeploymentTargetReadService>();

                        if (deploymentTargetReadService != null)
                        {
                            logger.Debug("Found deployment target read service of type {Type}",
                                deploymentTargetReadService.GetType().FullName);
                        }
                        else
                        {
                            logger.Debug("Could not find any deployment target read service");
                            return new DeploymentTargetIds(Array.Empty<string>());
                        }

                        IReadOnlyCollection<string> targetIds =
                            (await deploymentTargetReadService.GetDeploymentTargetsAsync(linkedToken.Token))
                            .Select(deploymentTarget => deploymentTarget.Id)
                            .ToArray();

                        logger.Debug("Found deployment target IDs {IDs}", targetIds);

                        return new DeploymentTargetIds(targetIds);
                    }
                }
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

            var excludedTypes = configuration.GetInstances<ExcludedAutoRegistrationType>()
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

        public static async Task<App> CreateAsync(
            CancellationTokenSource cancellationTokenSource,
            Action<LoggerConfiguration> loggerConfigurationAction,
            string[] args,
            ImmutableDictionary<string, string> environmentVariables)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            try
            {
                var app = await BuildAppAsync(cancellationTokenSource,
                    loggerConfigurationAction,
                    args,
                    environmentVariables);

                return app;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                TempLogger.WriteLine("Error in startup, " + ex);
                throw;
            }
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
                Logger.Fatal(ex, "Could not build web host {Application}", AppInstance);
                throw new DeployerAppException($"Could not build web host in {AppInstance}", ex);
            }

            if (args.Any(arg => arg.Equals(ApplicationConstants.RunAsService)))
            {
                Logger.Information("Starting {AppInstance} as a Windows Service", AppInstance);

                try
                {
                    WebHost.CustomRunAsService(this);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    Logger.Fatal(ex, "Could not start web host as a Windows service, {AppInstance}", AppInstance);
                    throw new DeployerAppException(
                        $"Could not start web host as a Windows service, configuration, {Configuration?.SourceChain} {AppInstance} ",
                        ex);
                }
            }
            else
            {
                Logger.Information("Starting as a Console Application, {AppInstance}", AppInstance);

                try
                {
                    await WebHost.StartAsync(CancellationTokenSource.Token);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    Logger.Fatal(ex, "Could not start web host, {AppInstance}", AppInstance);
                    throw new DeployerAppException(
                        $"Could not start web host, configuration {Configuration?.SourceChain} {AppInstance}",
                        ex);
                }
            }

            LogConfigurationValues();

            return 0;
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

            Logger?.Debug("Disposing application {Application} {Instance}",
                ApplicationConstants.ApplicationName,
                _instanceId);
            Logger?.Verbose("Disposing web host {Application} {Instance}",
                ApplicationConstants.ApplicationName,
                _instanceId);
            WebHost?.SafeDispose();
            Logger?.Verbose("Disposing Application root scope {Application} {Instance}",
                ApplicationConstants.ApplicationName,
                _instanceId);
            var rootScope = AppRootScope.Top();
            AppRootScope?.SafeDispose();
            rootScope?.SafeDispose();
            Logger?.Verbose("Disposing configuration {Application} {Instance}",
                ApplicationConstants.ApplicationName,
                _instanceId);
            Configuration?.SafeDispose();

            Logger?.Debug("Application disposal complete, disposing logging {Application} {Instance}",
                ApplicationConstants.ApplicationName,
                _instanceId);

            if (Logger is IDisposable disposable)
            {
                Logger?.Verbose("Disposing Logger {Application} {Instance}",
                    ApplicationConstants.ApplicationName,
                    _instanceId);
                disposable.SafeDispose();
            }
            else
            {
                Logger?.Debug("Logger is not disposable {Application} {Instance}",
                    ApplicationConstants.ApplicationName,
                    _instanceId);
            }

            Configuration = null;
            Logger = null;
            WebHost = null;
            HostBuilder = null;
            AppRootScope = null;
            _disposed = true;
            _disposing = false;
        }
    }
}
