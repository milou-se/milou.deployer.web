using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
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
        private readonly IMessageSink _diagnosticMessageSink;

        protected WebFixtureBase(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }
        public Exception Exception { get; private set; }

        private const int CancellationTimeoutInSeconds = 180;

        private CancellationTokenSource _cancellationTokenSource;

        [PublicAPI]
        public readonly List<FileInfo> FilesToClean = new List<FileInfo>();

        [PublicAPI]
        public readonly List<DirectoryInfo> DirectoriesToClean = new List<DirectoryInfo>();

        private PgServer _pgServer;

        public StringBuilder Builder { get; private set; }

        public App App { get; private set; }

        [PublicAPI]
        public int? HttpPort => GetHttpPort();

        private int? GetHttpPort()
        {
            var environmentConfiguration = App.AppRootScope.Lifetime.ResolveOptional<EnvironmentConfiguration>();

            if (environmentConfiguration is null)
            {
                return null;
            }

            return environmentConfiguration.HttpPort;
        }

        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private static readonly string PostgresqlUser = Environment.UserName; //"postgres";

        private const string ConnectionStringFormat = "Server=localhost;Port={0};User Id={1};Password=test;Database=postgres;Pooling=false";

        private bool AddLocalUserAccessPermission = true;

        public async Task InitializeAsync()
        {
            bool useDefaultDirectory = false;

            if (bool.TryParse(
                Environment.GetEnvironmentVariable("urn:milou:deployer:web:tests:pgsql:AddLocalUserAccessPermission"),
                out bool addUser))
            {
                AddLocalUserAccessPermission = addUser;
            }

            if (bool.TryParse(
                Environment.GetEnvironmentVariable("urn:milou:deployer:web:tests:pgsql:DefaultDirectory"),
                out bool useDefaultDirectoryEnabled))
            {
                useDefaultDirectory = useDefaultDirectoryEnabled;
            }

            string version = Environment.GetEnvironmentVariable("urn:milou:deployer:web:tests:pgsql:version")
                .WithDefault("10.5.1");

            DirectoryInfo postgresqlDbDir;
            if (useDefaultDirectory)
            {
                postgresqlDbDir = null;
            }
            else
            {
                postgresqlDbDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "tools",
                    "MysticMind.PostgresEmbed", version)).EnsureExists();
            }

            Console.WriteLine(typeof(MartenConfiguration));
            Console.WriteLine(typeof(Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers.DeployController));

            try
            {
                try
                {
                    _pgServer = new PgServer(
                        version,
                        PostgresqlUser,
                        dbDir: postgresqlDbDir?.FullName ?? "",
                        addLocalUserAccessPermission: AddLocalUserAccessPermission,
                        clearInstanceDirOnStop: true);
                    _pgServer.Start();
                }
                finally
                {
                    _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(CancellationTimeoutInSeconds));
                }

                string connStr = string.Format(ConnectionStringFormat, _pgServer.PgPort, PostgresqlUser);

                Environment.SetEnvironmentVariable("urn:milou:deployer:web:marten:singleton:connection-string", connStr);
                Environment.SetEnvironmentVariable("urn:milou:deployer:web:marten:singleton:enabled", "true");

                await BeforeInitialize(_cancellationTokenSource.Token);
                IReadOnlyCollection<string> args = await RunSetupAsync();

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new Core.DeployerAppException("The cancellation token is already cancelled, skipping before start");
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
                    throw new Core.DeployerAppException("Before start exception", ex);
                }

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new Core.DeployerAppException("The cancellation token is already cancelled, skipping start");
                }

                await StartAsync(args);

                await Task.Delay(TimeSpan.FromSeconds(1));

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new Core.DeployerAppException("The cancellation token is already cancelled, skipping run");
                }

                await RunAsync();

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new Core.DeployerAppException("The cancellation token is already cancelled, skipping after run");
                }

                await AfterRunAsync();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                Exception = ex;
                OnException(ex);
            }
        }

        protected virtual void OnException(Exception exception)
        {

        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            App?.Logger?.Information("Stopping app");
            _cancellationTokenSource?.Dispose();
            App?.Dispose();
            _pgServer?.Dispose();

            FileInfo[] files = FilesToClean.ToArray();

            foreach (FileInfo fileInfo in files)
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

            DirectoryInfo[] directoryInfos = DirectoriesToClean.ToArray();

            foreach (DirectoryInfo directoryInfo in directoryInfos)
            {
                try
                {
                    directoryInfo.Refresh();

                    if (directoryInfo.Exists)
                    {
                        directoryInfo.Delete(true);
                    }
                }
                catch (Exception)
                {
                    // ignore
                }

                DirectoriesToClean.Remove(directoryInfo);
            }
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

        private async Task StartAsync(IReadOnlyCollection<string> args)
        {
            App.Logger.Information("Starting app");

            await App.RunAsync(args.ToArray());

            App.Logger.Information("Started app, waiting for web host shutdown");
        }

        private async Task<IReadOnlyCollection<string>> RunSetupAsync()
        {
            string rootDirectory = VcsTestPathHelper.GetRootDirectory();

            string appRootDirectory = Path.Combine(rootDirectory, "src", "Milou.Deployer.Web.IisHost");

            string[] args =
            {
                $"{ConfigurationConstants.ContentBasePath}={appRootDirectory}",
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

            App = await App.CreateAsync(_cancellationTokenSource, AddTestLogging, args);

            App.Logger.Information("Restart time is set to {RestartIntervalInSeconds} seconds",
                CancellationTimeoutInSeconds);

            return args;
        }
    }
}