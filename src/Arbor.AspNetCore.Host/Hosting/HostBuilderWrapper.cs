using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbor.AspNetCore.Host.Hosting
{
    public sealed class HostBuilderWrapper : IHostBuilder
    {
        private readonly IHostBuilder _webHostBuilderImplementation;

        public HostBuilderWrapper([NotNull] IHostBuilder webHostBuilder) => _webHostBuilderImplementation =
            webHostBuilder ?? throw new ArgumentNullException(nameof(webHostBuilder));

        public IHost Build()
        {
            _webHostBuilderImplementation.ConfigureServices(services =>
                services.Add(new ServiceDescriptor(typeof(ServiceDiagnostics), ServiceDiagnostics.Create(services))));
            return new HostWrapper(_webHostBuilderImplementation.Build());
        }

        public IHostBuilder ConfigureAppConfiguration(
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            _webHostBuilderImplementation.ConfigureAppConfiguration(configureDelegate);

        public IHostBuilder ConfigureContainer<TContainerBuilder>(
            Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
            _webHostBuilderImplementation.ConfigureContainer(configureDelegate);

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
            _webHostBuilderImplementation.ConfigureHostConfiguration(configureDelegate);

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            _webHostBuilderImplementation.ConfigureServices(configureDelegate);

        public IHostBuilder
            UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) =>
            _webHostBuilderImplementation.UseServiceProviderFactory(factory);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) =>
            _webHostBuilderImplementation.UseServiceProviderFactory(factory);

        public IDictionary<object, object> Properties => _webHostBuilderImplementation.Properties;
    }
}