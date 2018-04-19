using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.AutofacConfiguration
{
    internal static class AutofacDependencyExtensions
    {
        public static void PopulateServices(this IComponentContext componentContext, IServiceCollection serviceCollection)
        {
            new AspNetServicesModule(serviceCollection).Configure(componentContext.ComponentRegistry);
        }
    }
}