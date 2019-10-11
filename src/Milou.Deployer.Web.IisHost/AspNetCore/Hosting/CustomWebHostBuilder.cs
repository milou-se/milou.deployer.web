using System.Collections.Generic;
using System.IO;
using System.Net;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Microsoft.Extensions.Configuration.Urns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Security;
using Milou.Deployer.Web.IisHost.AspNetCore.Startup;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    public static class CustomWebHostBuilder
    {
        public static IWebHostBuilder GetWebHostBuilder(
            EnvironmentConfiguration environmentConfiguration,
            IKeyValueConfiguration configuration,
            ServiceProviderHolder serviceProviderHolder,
            ILogger logger)
        {
            var contentRoot = environmentConfiguration?.ContentBasePath ?? Directory.GetCurrentDirectory();

            logger.Debug("Using content root {ContentRoot}", contentRoot);

            var kestrelServerOptions = new List<KestrelServerOptions>();

            var webHostBuilder = new WebHostBuilder()
                .ConfigureLogging((context, builder) => { builder.AddProvider(new SerilogLoggerProvider(logger)); })
                .ConfigureServices(services =>
                {
                    foreach (var serviceDescriptor in serviceProviderHolder.ServiceCollection)
                    {
                        services.Add(serviceDescriptor);
                    }

                    services.AddSingleton(environmentConfiguration);
                    services.AddHttpClient();

                    var openIdConnectConfiguration = serviceProviderHolder.ServiceProvider.GetService<CustomOpenIdConnectConfiguration>();

                    var httpLoggingConfiguration = serviceProviderHolder.ServiceProvider.GetService<HttpLoggingConfiguration>();

                    var milouAuthenticationConfiguration = serviceProviderHolder.ServiceProvider.GetService<MilouAuthenticationConfiguration>();

                    services.AddDeploymentAuthentication(openIdConnectConfiguration, milouAuthenticationConfiguration, logger, environmentConfiguration)
                        .AddDeploymentAuthorization(environmentConfiguration)
                        .AddDeploymentHttpClients(httpLoggingConfiguration)
                        .AddDeploymentSignalR()
                        .AddServerFeatures()
                        .AddDeploymentMvc(logger);

                    services.AddMvc();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddKeyValueConfigurationSource(configuration);

                    hostingContext.Configuration = new ConfigurationWrapper((IConfigurationRoot)hostingContext.Configuration, serviceProviderHolder);
                })
                .UseKestrel(options =>
                {
                    if (kestrelServerOptions.Contains(options))
                    {
                        return;
                    }

                    if (environmentConfiguration != null)
                    {
                        if (environmentConfiguration.UseExplicitPorts)
                        {
                            if (environmentConfiguration.HttpPort.HasValue)
                            {
                                logger.Information("Listening on http port {Port}",
                                    environmentConfiguration.HttpPort.Value);

                                options.Listen(IPAddress.Any,
                                    environmentConfiguration.HttpPort.Value);
                            }

                            if (environmentConfiguration.HttpsPort.HasValue
                                && environmentConfiguration.PfxFile.HasValue()
                                && environmentConfiguration.PfxPassword.HasValue())
                            {
                                logger.Information("Listening on https port {Port}",
                                    environmentConfiguration.HttpsPort.Value);

                                options.Listen(IPAddress.Any,
                                    environmentConfiguration.HttpsPort.Value,
                                    listenOptions =>
                                    {
                                        listenOptions.UseHttps(environmentConfiguration.PfxFile,
                                            environmentConfiguration.PfxPassword);
                                    });
                            }
                        }
                    }

                    kestrelServerOptions.Add(options);
                })
                .UseContentRoot(contentRoot)
                .ConfigureAppConfiguration((hostingContext, config) => { config.AddEnvironmentVariables(); })
                .UseIISIntegration()
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                })
                .UseStartup<ApplicationPipeline>();

            if (environmentConfiguration != null)
            {
                if (environmentConfiguration.EnvironmentName.HasValue())
                {
                    webHostBuilder = webHostBuilder.UseEnvironment(environmentConfiguration.EnvironmentName);
                }
            }

            var webHostBuilderWrapper = new WebHostBuilderWrapper(webHostBuilder);

            return webHostBuilderWrapper;
        }
    }
}
