using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.AutofacConfiguration
{
    public static class AutofacExtensions
    {
        public static IServiceCollection AddAutofacWithContainer(
            this IServiceCollection services,
            IComponentContext container)
        {
            var autofacOptions = new AutofacOptions(container, services);

            var instance = new AutofacServiceProviderFactory(autofacOptions);

            IServiceCollection serviceCollection = services.AddSingleton<IServiceProviderFactory<AutofacOptions>>(serviceProvider => instance);

            autofacOptions.UpdateServices();

            return serviceCollection;
        }

    }
}