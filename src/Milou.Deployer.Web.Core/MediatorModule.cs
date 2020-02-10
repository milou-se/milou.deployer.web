using System;
using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Milou.Deployer.Web.Core
{
    [UsedImplicitly]
    public class MediatorModule : IModule
    {
        private void RegisterTypes(
            Type openGenericType,
            IServiceCollection builder,
            ServiceLifetime serviceLifetime)
        {
            var types = ApplicationAssemblies.FilteredAssemblies()
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsPublic && t.IsConcrete())
                .Where(concreteType => concreteType.Closes(openGenericType))
                .ToArray();

            foreach (var implementationType in types)
            {
                builder.Add(new ExtendedServiceDescriptor(implementationType,
                    implementationType,
                    serviceLifetime,
                    GetType()));

                var interfaces = implementationType.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    builder.Add(new ExtendedServiceDescriptor(@interface,
                        context => context.GetService(implementationType),
                        serviceLifetime,
                        GetType()));
                }
            }
        }

        public IServiceCollection Register(IServiceCollection builder)
        {
            RegisterTypes(typeof(IPipelineBehavior<,>), builder, ServiceLifetime.Singleton);
            RegisterTypes(typeof(IRequestPostProcessor<,>), builder, ServiceLifetime.Singleton);
            RegisterTypes(typeof(IRequestPreProcessor<>), builder, ServiceLifetime.Singleton);
            RegisterTypes(typeof(INotificationHandler<>), builder, ServiceLifetime.Singleton);
            RegisterTypes(typeof(IRequestHandler<>), builder, ServiceLifetime.Singleton);
            RegisterTypes(typeof(IRequestHandler<,>), builder, ServiceLifetime.Singleton);

            builder.AddSingleton<ServiceFactory>(p => p.GetService, this);
            builder.AddSingleton<IMediator, Mediator>(this);

            builder.AddSingleton(typeof(IPipelineBehavior<,>),
                typeof(RequestPreProcessorBehavior<,>),
                this);

            builder.AddSingleton(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>), this);


            return builder;
        }
    }
}
