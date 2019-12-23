using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbor.AspNetCore.Host.Hosting
{
    [UsedImplicitly]
    public class BackgroundServiceModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            var types = ApplicationAssemblies.FilteredAssemblies()
                .GetLoadablePublicConcreteTypesImplementing<IHostedService>();

            foreach (var type in types)
            {
                builder.AddSingleton<IHostedService>(context => context.GetService(type), this);

                if (builder.Any(serviceDescriptor => serviceDescriptor.ImplementationType == type
                                                     && serviceDescriptor.ServiceType == type))
                {
                    continue;
                }

                builder.AddSingleton(type, this);
            }

            return builder;
        }
    }
}