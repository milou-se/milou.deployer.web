using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public sealed class WebHostBuilderWrapper : IWebHostBuilder
    {
        private readonly Scope _scope;
        private readonly IWebHostBuilder _webHostBuilderImplementation;

        public WebHostBuilderWrapper([NotNull] IWebHostBuilder webHostBuilder, Scope scope)
        {
            _webHostBuilderImplementation = webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));
            _scope = scope;
        }

        public IWebHost Build()
        {
            return new WebHostWrapper(_webHostBuilderImplementation.Build(), _scope);
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
