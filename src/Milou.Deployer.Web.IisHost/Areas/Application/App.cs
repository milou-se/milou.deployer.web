using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.IisHost.Areas.AspNet;
using Milou.Deployer.Web.IisHost.Areas.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Configuration.Modules;
using Milou.Deployer.Web.IisHost.Areas.Logging;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public sealed class App : IDisposable
    {
        public IContainer ComponentContext { get; private set; }

        [NotNull]
        private readonly CancellationTokenSource _cancellationTokenSource;

        public App([NotNull] IWebHostBuilder webHost, [NotNull] CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource ??
                                       throw new ArgumentNullException(nameof(cancellationTokenSource));
            HostBuilder = webHost ?? throw new ArgumentNullException(nameof(webHost));
        }

        public IWebHostBuilder HostBuilder { get; private set; }

        public IWebHost WebHost { get; private set; }

        public static async Task<App> CreateAsync(
            CancellationTokenSource cancellationTokenSource = default,
            Func<LoggerConfiguration, LoggerConfiguration> loggerConfigurationInterceptor = default,
            params string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            try
            {
                App app = BuildApp(cancellationTokenSource, args, loggerConfigurationInterceptor);

                return app;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error in startup");
                throw;
            }
        }

        public void Dispose()
        {
            Stop();

            ILogger logger = Log.Logger;

            Log.CloseAndFlush();

            if (logger is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ComponentContext?.Dispose();
            WebHost?.Dispose();
            WebHost = null;
            HostBuilder = null;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public async Task<int> RunAsync([NotNull] params string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            WebHost = HostBuilder.Build();

            await WebHost.StartAsync(_cancellationTokenSource.Token);

            await RunStartupTasks();

            return 0;
        }

        private static App BuildApp(
            CancellationTokenSource cancellationTokenSource,
            string[] args,
            Func<LoggerConfiguration, LoggerConfiguration> loggerConfigurationInterceptor)
        {
            string basePathFromArg = args.SingleOrDefault(arg =>
                arg.StartsWith("urn:milou:base-path", StringComparison.OrdinalIgnoreCase));

            string basePath = basePathFromArg?.Split('=').LastOrDefault() ?? AppDomain.CurrentDomain.BaseDirectory;

            SerilogApiInitialization.InitializeStartupLogging(file => GetBaseDirectoryFile(basePath, file));

            MultiSourceKeyValueConfiguration configuration =
                ConfigurationInitialization.InitializeConfiguration(file => GetBaseDirectoryFile(basePath, file));

            StaticKeyValueConfigurationManager.Initialize(configuration);

            SerilogApiInitialization.InitializeAppLogging(configuration, loggerConfigurationInterceptor);

            Log.Logger.Information("Started with command line args, {Args}", args);

            IContainer container = Bootstrapper.Start(basePath);

            ConfigureModules(configuration, cancellationTokenSource, container);

            var buildApp = container.Resolve<App>();

            buildApp.ComponentContext = container;

            return buildApp;
        }

        private static void ConfigureModules(
            MultiSourceKeyValueConfiguration configuration,
            [NotNull] CancellationTokenSource cancellationTokenSource,
            IComponentContext componentContext)
        {
            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            var loggingModule = new LoggingModule();

            loggingModule.Configure(componentContext.ComponentRegistry);

            var module = new KeyValueConfigurationModule(configuration);

            module.Configure(componentContext.ComponentRegistry);

            var urnModule = new UrnConfigurationModule(configuration);

            urnModule.Configure(componentContext.ComponentRegistry);

            var webHostModule = new AspNetWebHostModule(cancellationTokenSource, componentContext);

            webHostModule.Configure(componentContext.ComponentRegistry);

            var lifetimeModule = new LifetimeModule(cancellationTokenSource);

            lifetimeModule.Configure(componentContext.ComponentRegistry);
        }

        private async Task RunStartupTasks()
        {
            using (ILifetimeScope beginLifetimeScope = ComponentContext.BeginLifetimeScope())
            {
                var startupHandlers = beginLifetimeScope.Resolve<IEnumerable<IStartupHandler>>();

                foreach (IStartupHandler startupHandler in startupHandlers)
                {
                    try
                    {
                        await startupHandler.HandleAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex,
                            "Failed to run startup handler {Handler}",
                            startupHandler.GetType().FullName);
                        throw;
                    }
                }
            }
        }

        private static string GetBaseDirectoryFile(string basePath, string fileName)
        {
            return Path.Combine(basePath, fileName);
        }
    }
}