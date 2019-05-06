using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public class ServiceProviderHolder
    {
        public ServiceProviderHolder([NotNull] IServiceProvider serviceProvider, [NotNull] IServiceCollection serviceCollection)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public IServiceProvider ServiceProvider { get; }

        public IServiceCollection ServiceCollection { get; }
    }
}
