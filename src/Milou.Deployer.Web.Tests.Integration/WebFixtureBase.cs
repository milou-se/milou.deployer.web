using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers;
using Milou.Deployer.Web.Marten;
using MysticMind.PostgresEmbed;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Milou.Deployer.Web.Tests.Integration
{
    public abstract class WebFixtureBase : IDisposable, IAsyncLifetime
    {
        private const int CancellationTimeoutInSeconds = 180;

        private const string ConnectionStringFormat =
            "Server=localhost;Port={0};User Id={1};Password=test;Database=postgres;Pooling=false";

        private static readonly string PostgresqlUser = Environment.UserName; //"postgres";
        private readonly IMessageSink _diagnosticMessageSink;

        [PublicAPI]
        public readonly List<DirectoryInfo> DirectoriesToClean = new List<DirectoryInfo>();

        [PublicAPI]
        public List<FileInfo> FilesToClean { get; } = new List<FileInfo>();

        private CancellationTokenSource _cancellationTokenSource;
        private readonly DirectoryInfo _globalTempDir;
        private readonly string _oldTemp;

        private PgServer _pgServer;

        private bool _addLocalUserAccessPermission = true;

        protected WebFixtureBase(IMessageSink diagnosticMessageSink)
        {
            _globalTempDir =
                new DirectoryInfo(Path.Combine(Path.GetTempPath(), "mdst-" + Guid.NewGuid())).EnsureExists();

            _oldTemp = Path.GetTempPath();
            Environment.SetEnvironmentVariable("TEMP", _globalTempDir.FullName);
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public Exception Exception { get; private set; }

        public StringBuilder Builder { get; private set; }

        public App App { get; private set; }

        [PublicAPI]
        public int? HttpPort => GetHttpPort();

        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private int? GetHttpPort()
        {
            var environmentConfiguration = App.AppRootScope.Lifetime.ResolveOptional<EnvironmentConfiguration>();

            if (environmentConfiguration is null)
            {
                return null;
            }

            return environmentConfiguration.HttpPort;
        }

        private async Task DeleteDirectoryAsync(DirectoryInfo directoryInfo, int attempt = 0)
        {
            if (attempt == 5)
            {
                return;
            }

            try
            {
                directoryInfo.Refresh();

                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                }

                directoryInfo.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"could not delete directory {directoryInfo.FullName}", ex);
                // ignore

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }

            if (directoryInfo.Exists)
            {
                await DeleteDirectoryAsync(directoryInfo, attempt + 1);
            }
        }

        private async Task StartAsync(IReadOnlyCollection<string> args)
        {
            App.Logger.Information("Starting app");

            await App.RunAsync(args.ToArray());

            App.Logger.Information("Started app, waiting for web host shutdown");
        }

        private async Task<IReadOnlyCollection<string>> RunSetupAsync()
        {
            var rootDirectory = VcsTestPathHelper.GetRootDirectory();

            var appRootDirectory = Path.Combine(rootDirectory, "src", "Milou.Deployer.Web.IisHost");

            string[] args =
            {
                $"{ConfigurationConstants.ContentBasePath}={appRootDirectory}"
            };

            _cancellationTokenSource.Token.Register(() => Console.WriteLine("App cancellation token triggered"));

            Builder = new StringBuilder();
            var writer = new StringWriter(Builder);

            void AddTestLogging(LoggerConfiguration loggerConfiguration)
            {
                loggerConfiguration
                    .WriteTo.TextWriter(writer)
                    .WriteTo.Debug();
            }

            App = await App.CreateAsync(_cancellationTokenSource, AddTestLogging, args, EnvironmentVariables.Get());

            App.Logger.Information("Restart time is set to {RestartIntervalInSeconds} seconds",
                CancellationTimeoutInSeconds);

            return args;
        }

        public async Task InitializeAsync()
        {
            var useDefaultDirectory = false;

            if (bool.TryParse(
                Environment.GetEnvironmentVariable("urn:milou:deployer:web:tests:pgsql:AddLocalUserAccessPermission"),
                out var addUser))
            {
                _addLocalUserAccessPermission = addUser;
            }

            if (bool.TryParse(
                Environment.GetEnvironmentVariable("urn:milou:deployer:web:tests:pgsql:DefaultDirectory"),
                out var useDefaultDirectoryEnabled))
            {
                useDefaultDirectory = useDefaultDirectoryEnabled;
            }

            var version = Environment.GetEnvironmentVariable("urn:milou:deployer:web:tests:pgsql:version")
                .WithDefault("10.5.1");

            DirectoryInfo postgresqlDbDir;
            if (useDefaultDirectory)
            {
                postgresqlDbDir = null;
            }
            else
            {
                postgresqlDbDir = new DirectoryInfo(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "tools",
                    "MysticMind.PostgresEmbed",
                    version)).EnsureExists();
            }

            Console.WriteLine(typeof(MartenConfiguration));
            Console.WriteLine(typeof(DeployController));

            try
            {
                try
                {
                    _pgServer = new PgServer(
                        version,
                        PostgresqlUser,
                        postgresqlDbDir?.FullName ?? "",
                        addLocalUserAccessPermission: _addLocalUserAccessPermission,
                        clearInstanceDirOnStop: true);
                    _pgServer.Start();
                }
                finally
                {
                    _cancellationTokenSource =
                        new CancellationTokenSource(TimeSpan.FromSeconds(CancellationTimeoutInSeconds));
                }

                var connStr = string.Format(ConnectionStringFormat, _pgServer.PgPort, PostgresqlUser);

                Environment.SetEnvironmentVariable("urn:milou:deployer:web:marten:singleton:connection-string",
                    connStr);
                Environment.SetEnvironmentVariable("urn:milou:deployer:web:marten:singleton:enabled", "true");

                await BeforeInitialize(_cancellationTokenSource.Token);
                var args = await RunSetupAsync();

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new DeployerAppException(
                        "The cancellation token is already cancelled, skipping before start");
                }

                try
                {
                    _diagnosticMessageSink.OnMessage(new DiagnosticMessage("Running before start"));

                    await BeforeStartAsync(args);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _diagnosticMessageSink.OnMessage(new DiagnosticMessage(ex.ToString()));
                    _cancellationTokenSource.Cancel();
                    throw new DeployerAppException("Before start exception", ex);
                }

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new DeployerAppException("The cancellation token is already cancelled, skipping start");
                }

                await StartAsync(args);

                await Task.Delay(TimeSpan.FromSeconds(1));

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new DeployerAppException("The cancellation token is already cancelled, skipping run");
                }

                await RunAsync();

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new DeployerAppException("The cancellation token is already cancelled, skipping after run");
                }

                await AfterRunAsync();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                Exception = ex;
                OnException(ex);
            }
        }

        public virtual async Task DisposeAsync()
        {
            App?.Logger?.Information("Stopping app");
            _cancellationTokenSource?.Dispose();
            App?.Dispose();
            _pgServer?.Dispose();

            var files = FilesToClean.ToArray();

            foreach (var fileInfo in files)
            {
                try
                {
                    fileInfo.Refresh();
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }
                }
                catch (Exception)
                {
                    // ignore
                }

                FilesToClean.Remove(fileInfo);
            }

            var directoryInfos = DirectoriesToClean.ToArray();

            foreach (var directoryInfo in directoryInfos.OrderByDescending(x => x.FullName.Length))
            {
                await DeleteDirectoryAsync(directoryInfo);
                DirectoriesToClean.Remove(directoryInfo);
            }

            Environment.SetEnvironmentVariable("TEMP", _oldTemp);

            await DeleteDirectoryAsync(_globalTempDir);
        }

        public virtual void Dispose()
        {
        }

        protected virtual void OnException(Exception exception)
        {
        }

        protected virtual Task AfterRunAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task BeforeStartAsync(IReadOnlyCollection<string> args)
        {
            return Task.CompletedTask;
        }

        protected virtual Task BeforeInitialize(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected abstract Task RunAsync();
    }
}
