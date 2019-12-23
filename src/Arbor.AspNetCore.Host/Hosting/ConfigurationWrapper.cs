using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    public class ConfigurationWrapper : IConfigurationRoot
    {
        private readonly IConfigurationRoot _hostingContextConfiguration;

        public ConfigurationWrapper(
            IConfigurationRoot hostingContextConfiguration,
            ServiceProviderHolder serviceProviderHolder)
        {
            ServiceProviderHolder = serviceProviderHolder;
            _hostingContextConfiguration = hostingContextConfiguration;
        }

        public IEnumerable<IConfigurationProvider> Providers => _hostingContextConfiguration.Providers;

        public ServiceProviderHolder ServiceProviderHolder { get; }

        public string this[string key]
        {
            get => _hostingContextConfiguration[key];
            set => _hostingContextConfiguration[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _hostingContextConfiguration.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return _hostingContextConfiguration.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return _hostingContextConfiguration.GetSection(key);
        }

        public void Reload()
        {
            _hostingContextConfiguration.Reload();
        }
    }
}
