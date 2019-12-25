using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Microsoft.Extensions.Configuration.Urns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Arbor.AspNetCore.Host.Hosting
{
    public static class CustomWebHostBuilder<T> where T : class
    {
        public static IHostBuilder GetWebHostBuilder(EnvironmentConfiguration environmentConfiguration,
            IKeyValueConfiguration configuration,
            ServiceProviderHolder serviceProviderHolder,
            ILogger logger,
            string[] commandLineArgs,
            Action<IServiceCollection> onRegistration = null)
        {
            string contentRoot = environmentConfiguration?.ContentBasePath ?? Directory.GetCurrentDirectory();

            logger.Debug("Using content root {ContentRoot}", contentRoot);

            var kestrelServerOptions = new List<KestrelServerOptions>();

            IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(commandLineArgs);

            hostBuilder
                .ConfigureLogging((context, builder) => { builder.AddProvider(new SerilogLoggerProvider(logger)); })
                .ConfigureServices(services =>
                {
                    foreach (var serviceDescriptor in serviceProviderHolder.ServiceCollection)
                    {
                        services.Add(serviceDescriptor);
                    }

                    services.AddSingleton(environmentConfiguration);
                    services.AddHttpClient();

                    onRegistration?.Invoke(services);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddKeyValueConfigurationSource(configuration);

                    hostingContext.Configuration =
                        new ConfigurationWrapper((IConfigurationRoot)hostingContext.Configuration,
                            serviceProviderHolder);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
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
                        .UseStartup<T>();

                    if (environmentConfiguration != null)
                    {
                        if (environmentConfiguration.EnvironmentName.HasValue())
                        {
                            webBuilder.UseEnvironment(environmentConfiguration.EnvironmentName);
                        }
                    }
                });


            var webHostBuilderWrapper = new HostBuilderWrapper(hostBuilder);

            return webHostBuilderWrapper;
        }
    }
}