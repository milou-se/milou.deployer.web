using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arbor.App.Extensions.Application;
using Arbor.AspNetCore.Host.Hosting;
using Arbor.AspNetCore.Mvc.Formatting.HtmlForms.Core;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Extensions.BoolExtensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using Milou.Deployer.Web.Core.Json;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.Core.Security;
using Milou.Deployer.Web.IisHost.Areas.Logging;
using Milou.Deployer.Web.IisHost.Areas.Security;
using Milou.Deployer.Web.IisHost.Areas.Startup;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;
using MessageReceivedContext = Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext;
using TokenValidatedContext = Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
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
            CustomOpenIdConnectConfiguration openIdConnectConfiguration,
            MilouAuthenticationConfiguration milouAuthenticationConfiguration,
            ILogger logger,
            EnvironmentConfiguration environmentConfiguration)
        {
            var authenticationBuilder = serviceCollection
                .AddAuthentication(
                    option =>
                    {
                        option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                        if (openIdConnectConfiguration?.Enabled == true)
                        {
                            option.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        }
                        else
                        {
                            option.DefaultAuthenticateScheme = MilouAuthenticationConstants.MilouAuthenticationScheme;
                        }
                    })
                .AddCookie();

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

                        openIdConnectOptions.Events.OnRemoteFailure = context =>
                        {
                            logger.Error(context.Failure, "Remote call to OpenIDConnect {Uri} failed", context.Options.Backchannel.BaseAddress);

                            return Task.CompletedTask;
                        };

                        openIdConnectOptions.Events.OnRedirectToIdentityProvider = context =>
                        {
                            var redirectUrl = new Uri("http://localhost/signin-oidc");

                            UriBuilder builder = new UriBuilder(redirectUrl);

                            if (!string.IsNullOrWhiteSpace(environmentConfiguration.PublicHostname))
                            {
                                builder.Host = environmentConfiguration.PublicHostname;
                            }

                            if (environmentConfiguration.PublicPortIsHttps == true)
                            {
                                builder.Scheme = "https";
                                builder.Port = environmentConfiguration.HttpsPort ?? 443;
                            }

                            context.ProtocolMessage.RedirectUri = builder.Uri.AbsoluteUri;

                            return Task.CompletedTask;
                        };
                    });
            }

            if (milouAuthenticationConfiguration?.Enabled == true)
            {
                authenticationBuilder.AddMilouAuthentication(
                    MilouAuthenticationConstants.MilouAuthenticationScheme,
                    "Milou",
                    options => { });
            }

            if (milouAuthenticationConfiguration?.BearerTokenEnabled == true
             && !string.IsNullOrWhiteSpace(milouAuthenticationConfiguration.BearerTokenIssuerKey))
            {
                authenticationBuilder.AddJwtBearer(options =>
                {
                    var bytes = Convert.FromBase64String(milouAuthenticationConfiguration.BearerTokenIssuerKey);
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                            IssuerSigningKeys = new List<SecurityKey> { new SymmetricSecurityKey(bytes)},
                            ValidateAudience = false,
                            ValidateIssuer = false
                        };

                    options.TokenValidationParameters = tokenValidationParameters;

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = OnMessageReceived,
                        OnChallenge = OnChallenge,
                        OnTokenValidated = OnTokenValidated
                    };
                });
            }

            return serviceCollection;
        }

        private static Task OnChallenge(JwtBearerChallengeContext arg)
        {
            return Task.CompletedTask;
        }

        private static Task OnTokenValidated(TokenValidatedContext arg)
        {
            return Task.CompletedTask;
        }

        private static Task OnMessageReceived(MessageReceivedContext arg)
        {
            return Task.CompletedTask;
        }

        public static IServiceCollection AddDeploymentMvc(this IServiceCollection services,
            EnvironmentConfiguration environmentConfiguration,
            IKeyValueConfiguration configuration,
            ILogger logger)
        {
            var mvcBuilder = services.AddMvc(
                options =>
                {
                    options.InputFormatters.Insert(0, new XWwwFormUrlEncodedFormatter());
                }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.Converters.Add(new DateConverter());
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });

            foreach (var filteredAssembly in ApplicationAssemblies.FilteredAssemblies(useCache: false))
            {
                logger.Debug("Adding assembly {Assembly} to MVC application parts", filteredAssembly.FullName);
                mvcBuilder.AddApplicationPart(filteredAssembly);
            }

            services.AddControllers();
            var razorPagesBuilder = services.AddRazorPages();

#if DEBUG
            if (environmentConfiguration.ToHostEnvironment().IsDevelopment()
                || configuration.ValueOrDefault(StartupConstants.RuntimeCompilationEnabled, false))
            {
                razorPagesBuilder.AddRazorRuntimeCompilation();
            }
#endif

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
                    options.AddPolicy(
                        AuthorizationPolicies.Agent,
                        policy =>
                        {
                            policy.Requirements.Add(new AgentAuthorizationRequirement());
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        });
                });

            services.AddSingleton<IAuthorizationHandler, DefaultAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, AgentAuthorizationHandler>();

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
