using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    public sealed class WebHostBuilderWrapper : IWebHostBuilder
    {
        private readonly IWebHostBuilder _webHostBuilderImplementation;

        public WebHostBuilderWrapper([NotNull] IWebHostBuilder webHostBuilder)
        {
            _webHostBuilderImplementation = webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));
        }

        public IWebHost Build()
        {
            _webHostBuilderImplementation.ConfigureServices(services =>
                services.Add(new ServiceDescriptor(typeof(ServiceDiagnostics), ServiceDiagnostics.Create(services))));
            return new WebHostWrapper(_webHostBuilderImplementation.Build());
        }

        public IWebHostBuilder ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return _webHostBuilderImplementation.ConfigureAppConfiguration(configureDelegate);
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _webHostBuilderImplementation.ConfigureServices(configureServices);
        }

        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            return _webHostBuilderImplementation.ConfigureServices(configureServices);
        }

        public string GetSetting(string key)
        {
            return _webHostBuilderImplementation.GetSetting(key);
        }

        public IWebHostBuilder UseSetting(string key, string value)
        {
            return _webHostBuilderImplementation.UseSetting(key, value);
        }
    }
}
