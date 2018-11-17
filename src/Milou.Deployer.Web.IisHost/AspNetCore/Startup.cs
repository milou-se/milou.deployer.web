﻿using System;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Logging;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class Startup
    {
        [NotNull]
        private readonly HttpLoggingConfiguration _httpLoggingConfiguration;

        [NotNull]
        private readonly EnvironmentConfiguration _environmentConfiguration;

        [NotNull]
        private readonly Serilog.ILogger _logger;

        [NotNull]
        private readonly Scope _webHostScope;

        private IDisposable _disposableScope;

        public Startup(
            [NotNull] Scope webHostScope,
            [NotNull] Serilog.ILogger logger,
            [NotNull] HttpLoggingConfiguration httpLoggingConfiguration,
            [NotNull] EnvironmentConfiguration environmentConfiguration)
        {
            _webHostScope = webHostScope.Deepest() ?? throw new ArgumentNullException(nameof(webHostScope));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpLoggingConfiguration = httpLoggingConfiguration;
            _environmentConfiguration = environmentConfiguration ?? throw new ArgumentNullException(nameof(environmentConfiguration));
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDeploymentAuthentication();

            services.AddDeploymentAuthorization();

            services.AddDeploymentHttpClients(_httpLoggingConfiguration);

            services.AddMvc();

            services.AddDeploymentSignalR();

            services.AddServerFeatures();

            services.AddDeploymentMvc(_logger);

            Scope scope = services.AddScopeModules(_webHostScope, _logger);

            _disposableScope = scope.Lifetime;

            return new AutofacServiceProvider(scope.Lifetime);
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            app.AddForwardHeaders();

            app.AddExceptionHandling(_environmentConfiguration);

            app.UseAuthentication();

            app.UseSignalRHubs();

            app.UseStaticFiles();

            app.UseMvc();

            appLifetime.ApplicationStopped.Register(() => _disposableScope?.Dispose());
        }
    }
}