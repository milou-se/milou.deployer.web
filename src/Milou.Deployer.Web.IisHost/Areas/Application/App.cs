using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Arbor.Tooler;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.Core.NuGet;
using Milou.Deployer.Web.IisHost.Areas.Configuration;
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
            MultiSourceKeyValueConfiguration configuration,
            ConfigurationInstanceHolder configurationInstanceHolder)
        {
            CancellationTokenSource = cancellationTokenSource ??
                                      throw new ArgumentNullException(nameof(cancellationTokenSource));
            Logger = appLogger ?? throw new ArgumentNullException(nameof(appLogger));
            Configuration = configuration;
            ConfigurationInstanceHolder = configurationInstanceHolder;
            HostBuilder = webHost ?? throw new ArgumentNullException(nameof(webHost));
            _instanceId = Guid.NewGuid();
            AppInstance = ApplicationConstants.ApplicationName + " " + _instanceId;
        }

        public string AppInstance { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        public ILogger Logger { get; private set; }

        public MultiSourceKeyValueConfiguration Configuration { get; private set; }

        public ConfigurationInstanceHolder ConfigurationInstanceHolder { get; }

        [PublicAPI]
        public IWebHostBuilder HostBuilder { get; private set; }

        public IWebHost WebHost { get; private set; }

        private static async Task<App> BuildAppAsync(
            CancellationTokenSource cancellationTokenSource,
            string[] commandLineArgs,
            ImmutableDictionary<string, string> environmentVariables,
            params object[] instances)
        {
            var scanAssemblies = Assemblies.FilteredAssemblies().ToArray();

            MultiSourceKeyValueConfiguration startupConfiguration = ConfigurationInitialization.InitializeStartupConfiguration(commandLineArgs, environmentVariables, scanAssemblies);

            ConfigurationRegistrations startupRegistrations = startupConfiguration.ScanRegistrations(scanAssemblies);

            if (startupRegistrations.UrnTypeRegistrations
                .Any(registrationErrors => !registrationErrors.ConfigurationRegistrationErrors.IsDefaultOrEmpty))
            {
                var errors = startupRegistrations.UrnTypeRegistrations
                    .Where(registrationErrors => registrationErrors.ConfigurationRegistrationErrors.Length > 0)
                    .SelectMany(registrationErrors => registrationErrors.ConfigurationRegistrationErrors)
                    .Select(registrationError => registrationError.ErrorMessage);

                throw new InvalidOperationException($"Error {string.Join(Environment.NewLine, errors)}"); // review exception
            }

            ConfigurationInstanceHolder configurationInstanceHolder = startupRegistrations.CreateHolder();

            configurationInstanceHolder.AddInstance(configurationInstanceHolder);

            foreach (object instance in instances)
            {
                configurationInstanceHolder.AddInstance(instance);
            }

            var loggingLevelSwitch = new LoggingLevelSwitch();
            configurationInstanceHolder.AddInstance(loggingLevelSwitch);
            configurationInstanceHolder.AddInstance(cancellationTokenSource);
            configurationInstanceHolder.AddInstance(new NuGetConfiguration());

            ApplicationPaths paths = configurationInstanceHolder.GetInstances<ApplicationPaths>().SingleOrDefault().Value ?? new ApplicationPaths();

            AppPathHelper.SetApplicationPaths(paths, commandLineArgs);

            var startupLoggerConfigurationHandlers = Assemblies.FilteredAssemblies()
                .GetLoadablePublicConcreteTypesImplementing<IStartupLoggerConfigurationHandler>()
                .Select(type => configurationInstanceHolder.Create(type).Cast<IStartupLoggerConfigurationHandler>())
                .ToImmutableArray();

            SetLoggingLevelSwitch(loggingLevelSwitch, startupConfiguration);

            var startupLogger =
                SerilogApiInitialization.InitializeStartupLogging(
                    file => GetBaseDirectoryFile(paths.BasePath, file),
                    environmentVariables,
                    startupLoggerConfigurationHandlers);

            MultiSourceKeyValueConfiguration configuration;
            try
            {
                configuration =
                    ConfigurationInitialization.InitializeConfiguration(file => GetBaseDirectoryFile(paths.BasePath, file),
                        paths.ContentBasePath,
                        scanAssemblies,
                        commandLineArgs,
                        environmentVariables);

                configurationInstanceHolder.AddInstance(configuration);
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

            TempPathHelper.SetTempPath(configuration, startupLogger);

            SetLoggingLevelSwitch(loggingLevelSwitch, configuration);

            var loggerConfigurationHandlers = Assemblies.FilteredAssemblies()
                .GetLoadablePublicConcreteTypesImplementing<ILoggerConfigurationHandler>()
                .Select(type => configurationInstanceHolder.Create(type).Cast<ILoggerConfigurationHandler>());

            ILogger appLogger;
            try
            {
                appLogger =
                    SerilogApiInitialization.InitializeAppLogging(
                        configuration,
                        startupLogger,
                        loggerConfigurationHandlers,
                        loggingLevelSwitch);

                configurationInstanceHolder.AddInstance(appLogger);
            }
            catch (Exception ex)
            {
                appLogger = startupLogger;
                startupLogger.Error(ex, "Could not create app logger");
            }

            LogCommandLineArgs(commandLineArgs, appLogger);

            var environmentConfiguration = new EnvironmentConfiguration
            {
                ApplicationBasePath = paths.BasePath,
                ContentBasePath = paths.ContentBasePath,
                CommandLineArgs = commandLineArgs.ToImmutableArray(),
                EnvironmentName = environmentVariables.ValueOrDefault(ApplicationConstants.AspNetEnvironment)
            };

            configurationInstanceHolder.AddInstance(environmentConfiguration);

            IReadOnlyList<IModule> modules = GetConfigurationModules(configurationInstanceHolder, scanAssemblies);

            ServiceCollection serviceCollection = new ServiceCollection();

            try
            {
                Bootstrapper.Start(modules, serviceCollection, appLogger);
            }
            catch (Exception ex)
            {
                appLogger.Fatal(ex, "Could not initialize container registrations");
                throw;
            }

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

            var nugetConfiguration = configurationInstanceHolder.Get<NuGetConfiguration>();

            nugetConfiguration.NugetExePath = nugetExePath;

            EnvironmentConfigurator.ConfigureEnvironment(configurationInstanceHolder);

            foreach (Type registeredType in configurationInstanceHolder.RegisteredTypes)
            {
                var interfaces = registeredType.GetInterfaces();

                var instance = configurationInstanceHolder.GetInstances(registeredType).Single().Value;

                foreach (var @interface in interfaces)
                {
                    serviceCollection.AddSingleton(@interface, context => instance);
                }

                var serviceType = instance.GetType();
                serviceCollection.AddSingleton(serviceType, instance);
            }

            var app = new App(CustomWebHostBuilder.GetWebHostBuilder(environmentConfiguration, configuration, new ServiceProviderHolder(serviceCollection.BuildServiceProvider(), serviceCollection), appLogger), cancellationTokenSource, appLogger, configuration, configurationInstanceHolder);

            return app;
        }

        private static void LogCommandLineArgs(string[] commandLineArgs, ILogger appLogger)
        {
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
        }

        private static void SetLoggingLevelSwitch(LoggingLevelSwitch loggingLevelSwitch, MultiSourceKeyValueConfiguration configuration)
        {
            var defaultLevel = configuration[ConfigurationConstants.LogLevel]
                .ParseOrDefault(LogEventLevel.Information);

            loggingLevelSwitch.MinimumLevel = defaultLevel;
        }


        private static ImmutableArray<IModule> GetConfigurationModules(
            ConfigurationInstanceHolder holder,
            Assembly[] scanAssemblies)
        {
            var moduleTypes = scanAssemblies
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => type.IsPublicConcreteTypeImplementing<IModule>())
                .ToImmutableArray();

            var modules = moduleTypes
                .Select(moduleType =>
                    holder.Create(moduleType) as IModule)
                .Where(instance => instance is object)
                .ToImmutableArray();

            return modules;
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
            string[] args,
            ImmutableDictionary<string, string> environmentVariables,
            params object[] instances)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            try
            {
                var app = await BuildAppAsync(
                    cancellationTokenSource,
                    args,
                    environmentVariables,
                    instances);

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

            if (args.Any(arg => arg.Equals(ApplicationConstants.RunAsService, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Information("Starting {AppInstance} as a Windows Service", AppInstance);

                try
                {
                    //WebHost.CustomRunAsService(this);
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
            _disposed = true;
            _disposing = false;
        }
    }

    public class ServiceProviderHolder
    {
        public IServiceProvider ServiceProvider { get; }
        public IServiceCollection ServiceCollection { get; }

        public ServiceProviderHolder(IServiceProvider serviceProvider, IServiceCollection serviceCollection)
        {
            ServiceProvider = serviceProvider;
            ServiceCollection = serviceCollection;
        }
    }
}
