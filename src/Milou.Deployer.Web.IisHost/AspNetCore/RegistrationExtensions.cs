using System;
using System.Threading.Tasks;
using Arbor.AspNetCore.Mvc.Formatting.HtmlForms.Core;
using JetBrains.Annotations;
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
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Json;
using Milou.Deployer.Web.Core.Logging;
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

            if (openIdConnectConfiguration?.Enabled == true)
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
                                context.ProtocolMessage.RedirectUri =
                                    openIdConnectConfiguration.RedirectUri.AbsoluteUri;
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
    }
}
