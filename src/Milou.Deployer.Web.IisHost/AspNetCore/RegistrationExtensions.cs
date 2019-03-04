using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arbor.AspNetCore.Mvc.Formatting.HtmlForms.Core;
using Arbor.KVConfiguration.Core;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Json;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Configuration.Modules;
using Milou.Deployer.Web.IisHost.Areas.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.Security;
using Newtonsoft.Json;
using Serilog.AspNetCore;
using ILogger = Serilog.ILogger;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddDeploymentHttpClients(
            this IServiceCollection services,
            [NotNull] HttpLoggingConfiguration httpLoggingConfiguration)
        {
            if (httpLoggingConfiguration == null)
            {
                throw new ArgumentNullException(nameof(httpLoggingConfiguration));
            }

            services.AddHttpClient();

            if (!httpLoggingConfiguration.Enabled)
            {
                services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());
            }

            return services;
        }

        public static IServiceCollection AddDeploymentAuthentication(
            this IServiceCollection serviceCollection,
            CustomOpenIdConnectConfiguration openIdConnectConfiguration)
        {
            var authenticationBuilder = serviceCollection.AddAuthentication(
                option =>
                {
                    option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    option.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                }).AddCookie();

            if (openIdConnectConfiguration.Enabled)
            {
                authenticationBuilder = authenticationBuilder.AddOpenIdConnect(
                    openIdConnectOptions =>
                    {
                        openIdConnectOptions.ClientId = openIdConnectConfiguration.ClientId;
                        openIdConnectOptions.ClientSecret = openIdConnectConfiguration.ClientSecret;
                        openIdConnectOptions.Authority = openIdConnectConfiguration.Authority;
                        openIdConnectOptions.ResponseType = "code";
                        openIdConnectOptions.GetClaimsFromUserInfoEndpoint = true;
                        openIdConnectOptions.MetadataAddress = openIdConnectConfiguration.MetadataAddress;
                        openIdConnectOptions.Scope.Add("email");
                        openIdConnectOptions.TokenValidationParameters.ValidIssuer = openIdConnectConfiguration.Issuer;
                        openIdConnectOptions.TokenValidationParameters.IssuerValidator = (issuer, token, parameters) =>
                        {
                            if (string.Equals(issuer, openIdConnectConfiguration.Issuer, StringComparison.Ordinal))
                            {
                                return issuer;
                            }

                            throw new InvalidOperationException("Invalid issuer");
                        };
                        openIdConnectOptions.Events.OnRedirectToIdentityProvider = context =>
                        {
                            if (openIdConnectConfiguration.RedirectUri.HasValue())
                            {
                                context.ProtocolMessage.RedirectUri = openIdConnectConfiguration.RedirectUri;
                            }

                            return Task.CompletedTask;
                        };
                    });
            }

            authenticationBuilder.AddMilouAuthentication(
                MilouAuthenticationConstants.MilouAuthenticationScheme,
                "Milou",
                options => { });

            return serviceCollection;
        }

        public static IServiceCollection AddDeploymentMvc(this IServiceCollection services, ILogger logger)
        {
            services.AddMvc(
                options =>
                {
                    options.InputFormatters.Insert(
                        0,
                        new XWwwFormUrlEncodedFormatter(
                            new SerilogLoggerFactory(logger).CreateLogger<XWwwFormUrlEncodedFormatter>()));
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.Converters.Add(new DateConverter());
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });

            return services;
        }

        public static IServiceCollection AddServerFeatures(this IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IServerAddressesFeature, ServerAddressesFeature>();

            return services;
        }

        public static IServiceCollection AddDeploymentAuthorization(
            this IServiceCollection services,
            EnvironmentConfiguration environmentConfiguration)
        {
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(
                        AuthorizationPolicies.IpOrToken,
                        policy => policy.Requirements.Add(new DefaultAuthorizationRequirement()));
                });

            services.AddSingleton<IAuthorizationHandler, DefaultAuthorizationHandler>();

            if (environmentConfiguration.IsDevelopmentMode)
            {
                services.AddSingleton<IAuthorizationHandler, DevelopmentPermissionHandler>();
            }

            return services;
        }

        public static IServiceCollection AddDeploymentSignalR(this IServiceCollection services)
        {
            services.AddSignalR(
                options =>
                {
                    options.EnableDetailedErrors = true;
                    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
                });

            return services;
        }

        public static Scope AddScopeModules(this IServiceCollection services, Scope webHostScope, ILogger logger)
        {
            var deploymentTargetIds = webHostScope.Lifetime.Resolve<DeploymentTargetIds>();

            var aspNetScopeLifetimeScope = webHostScope.Lifetime.BeginLifetimeScope(
                Scope.AspNetCoreScope,
                builder =>
                {
                    foreach (var deploymentTargetId in deploymentTargetIds.DeploymentWorkerIds)
                    {
                        builder.Register(
                                context => new DeploymentTargetWorker(
                                    deploymentTargetId,
                                    context.Resolve<DeploymentService>(),
                                    context.Resolve<ILogger>(),
                                    context.Resolve<IMediator>(),
                                    context.Resolve<WorkerConfiguration>())).AsSelf().AsImplementedInterfaces()
                            .Named<DeploymentTargetWorker>(deploymentTargetId);
                    }

                    builder.Register(
                            context => new DeploymentWorker(context.Resolve<IEnumerable<DeploymentTargetWorker>>()))
                        .AsSelf().AsImplementedInterfaces().SingleInstance();

                    var keyValueConfiguration = webHostScope.Lifetime.Resolve<IKeyValueConfiguration>();

                    try
                    {
                        if (webHostScope.Lifetime.ResolveOptional<IDeploymentTargetReadService>() is null)
                        {
                            builder.RegisterModule(new AppServiceModule(keyValueConfiguration, logger));
                        }
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        logger.Warning(ex, "Could not get deployment target read service, registering defaults");
                        builder.RegisterModule(new AppServiceModule(keyValueConfiguration, logger));
                    }

                    var orderedModuleRegistrations = webHostScope.Lifetime
                        .Resolve<IReadOnlyCollection<OrderedModuleRegistration>>()
                        .Where(orderedModuleRegistration => orderedModuleRegistration.ModuleRegistration.Tag != null)
                        .OrderBy(orderedModuleRegistration => orderedModuleRegistration.ModuleRegistration.Order)
                        .ToArray();

                    foreach (var module in orderedModuleRegistrations)
                    {
                        module.Module.RegisterModule(Scope.AspNetCoreScope, builder, logger);
                    }

                    builder.Populate(services);
                });

            var aspNetCoreScope = new Scope(Scope.AspNetCoreScope, aspNetScopeLifetimeScope);
            webHostScope.Deepest().SubScope = aspNetCoreScope;

            return aspNetCoreScope;
        }
    }
}
