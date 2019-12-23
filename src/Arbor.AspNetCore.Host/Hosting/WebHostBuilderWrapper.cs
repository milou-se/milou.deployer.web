using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.AspNetCore.Host.Hosting
{
    public sealed class WebHostBuilderWrapper : IWebHostBuilder
    {
        private readonly IWebHostBuilder _webHostBuilderImplementation;

        public WebHostBuilderWrapper([NotNull] IWebHostBuilder webHostBuilder) => _webHostBuilderImplementation =
            webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));

        public IWebHost Build()
        {
            _webHostBuilderImplementation.ConfigureServices(services =>
                services.Add(new ServiceDescriptor(typeof(ServiceDiagnostics), ServiceDiagnostics.Create(services))));
            return new WebHostWrapper(_webHostBuilderImplementation.Build());
        }

        public IWebHostBuilder ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            _webHostBuilderImplementation.ConfigureAppConfiguration(configureDelegate);

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices) =>
            _webHostBuilderImplementation.ConfigureServices(configureServices);

        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices) =>
            _webHostBuilderImplementation.ConfigureServices(configureServices);

        public string GetSetting(string key) => _webHostBuilderImplementation.GetSetting(key);

        public IWebHostBuilder UseSetting(string key, string value) =>
            _webHostBuilderImplementation.UseSetting(key, value);
    }
}