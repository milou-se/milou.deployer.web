using System;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.AutofacConfiguration
{
    public class AutofacServiceProviderFactory : IServiceProviderFactory<AutofacOptions>
    {
        public AutofacOptions AutofacOptions { get; }

        public AutofacServiceProviderFactory([NotNull] AutofacOptions autofacOptions)
        {
            AutofacOptions = autofacOptions ?? throw new ArgumentNullException(nameof(autofacOptions));
        }

        public AutofacOptions CreateBuilder([NotNull] IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            AutofacOptions.ComponentContext.PopulateServices(services);

            return AutofacOptions;
        }

        public IServiceProvider CreateServiceProvider(AutofacOptions registry)
        {
            return new AutofacServiceProvider(AutofacOptions.ComponentContext);
        }
    }
}