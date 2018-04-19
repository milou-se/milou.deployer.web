using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.AutofacConfiguration
{
    public class AutofacOptions
    {
        public AutofacOptions(IComponentContext componentContext, IServiceCollection services)
        {
            ComponentContext = componentContext;
            Services = services;
        }

        public IComponentContext ComponentContext { get; }

        public IServiceCollection Services { get; }

        public void UpdateServices()
        {
            ComponentContext.PopulateServices(Services);
        }
    }
}