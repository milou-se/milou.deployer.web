using System;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Logging;
using Serilog;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class Startup
    {
        [NotNull]
        private readonly EnvironmentConfiguration _environmentConfiguration;

        [NotNull]
        private readonly HttpLoggingConfiguration _httpLoggingConfiguration;

        [NotNull]
        private readonly ILogger _logger;

        private readonly CustomOpenIdConnectConfiguration _openIdConnectConfiguration;

        [NotNull]
        private readonly Scope _webHostScope;

        private IDisposable _disposableScope;

        public Startup(
            [NotNull] Scope webHostScope,
            [NotNull] ILogger logger,
            [NotNull] HttpLoggingConfiguration httpLoggingConfiguration,
            [NotNull] EnvironmentConfiguration environmentConfiguration,
            CustomOpenIdConnectConfiguration openIdConnectConfiguration)
        {
            _webHostScope = webHostScope.Deepest() ?? throw new ArgumentNullException(nameof(webHostScope));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpLoggingConfiguration = httpLoggingConfiguration;
            _environmentConfiguration = environmentConfiguration ??
                                        throw new ArgumentNullException(nameof(environmentConfiguration));
            _openIdConnectConfiguration = openIdConnectConfiguration;
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDeploymentAuthentication(_openIdConnectConfiguration)
                .AddDeploymentAuthorization(_environmentConfiguration)
                .AddDeploymentHttpClients(_httpLoggingConfiguration)
                .AddDeploymentSignalR()
                .AddServerFeatures()
                .AddDeploymentMvc(_logger);

            services.AddMvc();

            var scope = services.AddScopeModules(_webHostScope, _logger);

            _disposableScope = scope.Lifetime;

            return new AutofacServiceProvider(scope.Lifetime);
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            app.AddForwardHeaders(_environmentConfiguration);

            app.AddRequestLogging(_environmentConfiguration);

            app.AddExceptionHandling(_environmentConfiguration);

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMiddleware<ConfigurationErrorMiddleware>();

            app.UseSignalRHubs();

            app.UseMvc();

            appLifetime.ApplicationStopped.Register(() => _disposableScope?.Dispose());
        }
    }
}
