using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.DependencyInjection;
using Milou.Deployer.Web.Core.Extensions;

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

    public class ServiceDiagnostics
    {
        public ImmutableArray<ServiceRegistrationInfo> Registrations { get; }

        private ServiceDiagnostics(IEnumerable<ServiceRegistrationInfo> registrations)
        {
            Registrations = registrations.SafeToImmutableArray();
        }

        public static ServiceDiagnostics Create(IServiceCollection services)
        {
            IEnumerable<ServiceRegistrationInfo> registrations = services.Select(ServiceRegistrationInfo.Create);

            return new ServiceDiagnostics(registrations);
        }
    }

    public class ServiceRegistrationInfo
    {
        public Type ServiceDescriptorServiceType { get; }
        public Type ServiceDescriptorImplementationType { get; }
        public object ServiceDescriptorImplementationInstance { get; }
        public ServiceLifetime ServiceDescriptorLifetime { get; }
        public Func<IServiceProvider, object> Factory { get; }
        public Type Module { get; }

        private ServiceRegistrationInfo(Type serviceDescriptorServiceType, Type serviceDescriptorImplementationType, object serviceDescriptorImplementationInstance, ServiceLifetime serviceDescriptorLifetime, Func<IServiceProvider, object> factory, Type module)
        {
            ServiceDescriptorServiceType = serviceDescriptorServiceType;
            ServiceDescriptorImplementationType = serviceDescriptorImplementationType;
            ServiceDescriptorImplementationInstance = serviceDescriptorImplementationInstance;
            ServiceDescriptorLifetime = serviceDescriptorLifetime;
            Factory = factory;
            Module = module;
        }

        public static ServiceRegistrationInfo Create(ServiceDescriptor serviceDescriptor)
        {
            Type module = null;

            if (serviceDescriptor is ExtendedServiceDescriptor extendedServiceDescriptor)
            {
                module = extendedServiceDescriptor.ModuleType;
            }

            return new ServiceRegistrationInfo(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.ImplementationInstance, serviceDescriptor.Lifetime, serviceDescriptor.ImplementationFactory, module);
        }
    }
}
