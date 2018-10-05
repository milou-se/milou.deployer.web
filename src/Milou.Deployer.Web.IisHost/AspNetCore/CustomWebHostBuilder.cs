using System.Collections.Generic;
using System.IO;
using System.Net;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Serilog.Extensions.Logging;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public static class CustomWebHostBuilder
    {
        public static IWebHostBuilder GetWebHostBuilder(
            Scope startupScope,
            Scope webHostScope,
            Serilog.ILogger logger)
        {
            var environmentConfiguration =
                startupScope.Deepest().Lifetime.ResolveOptional<EnvironmentConfiguration>();

            string contentRoot = environmentConfiguration?.ContentBasePath ?? Directory.GetCurrentDirectory();

            var kestrelServerOptions = new List<KestrelServerOptions>();

            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureLogging((context, builder) => { builder.AddProvider(new SerilogLoggerProvider(logger)); })
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddTransient(provider => webHostScope.Lifetime.Resolve<Startup>());
                })
                .UseKestrel(options =>
                {
                    if (kestrelServerOptions.Contains(options))
                    {
                        return;
                    }

                    if (environmentConfiguration != null)
                    {
                        if (environmentConfiguration.HttpPort.HasValue)
                        {
                            options.Listen(IPAddress.Loopback,
                                environmentConfiguration.HttpPort.Value);
                        }

                        if (environmentConfiguration.HttpsPort.HasValue &&
                            environmentConfiguration.PfxFile.HasValue() &&
                            environmentConfiguration.PfxPassword.HasValue())
                        {
                            options.Listen(IPAddress.Loopback,
                                environmentConfiguration.HttpsPort.Value,
                                listenOptions =>
                                {
                                    listenOptions.UseHttps(environmentConfiguration.PfxFile,
                                        environmentConfiguration.PfxPassword);
                                });
                        }
                    }

                    kestrelServerOptions.Add(options);
                })
                .UseContentRoot(contentRoot)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .UseIISIntegration()
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                })
                .UseStartup<Startup>();

            if (environmentConfiguration != null)
            {
                if (environmentConfiguration.EnvironmentName.HasValue())
                {
                    webHostBuilder = webHostBuilder.UseEnvironment(environmentConfiguration.EnvironmentName);
                }
            }

            return webHostBuilder;
        }
    }
}
