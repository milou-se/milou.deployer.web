using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Configuration.AutofacConfiguration;
using Serilog;
using Serilog.Extensions.Logging;

namespace Milou.Deployer.Web.IisHost.Areas.AspNet
{
    public static class CustomWebHostBuilder
    {
        public static IWebHostBuilder GetWebHostBuilder(IComponentContext componentContext)
        {
            var environmentConfiguration =
                componentContext.ResolveOptional<EnvironmentConfiguration>();

            string contentRoot = environmentConfiguration?.ApplicationBasePath ?? Directory.GetCurrentDirectory();

            var kestrelServerOptions = new List<KestrelServerOptions>();

            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddAutofacWithContainer(componentContext); })
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
                    IHostingEnvironment hostingEnvironment = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile(
                            $"appsettings.{hostingEnvironment.EnvironmentName}.json",
                            true,
                            true);

                    if (hostingEnvironment.IsDevelopment())
                    {
                        Assembly assembly = typeof(CustomWebHostBuilder).Assembly;
                        config.AddUserSecrets(assembly, true);
                    }

                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, builder) => builder.AddProvider(new SerilogLoggerProvider(Log.Logger)))
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