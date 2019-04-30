using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.Core
{
    [UsedImplicitly]
    public class MediatorModule : IModule
    {
        private readonly IReadOnlyCollection<Type> _excludedTypes;
        private readonly ILogger _logger;

        public MediatorModule(ILogger logger)
        {
            _logger = logger;
        }

        private IServiceCollection RegisterTypes(Type openGenericType, IServiceCollection builder, ServiceLifetime serviceLifetime)
        {
            var types = Assemblies.FilteredAssemblies()
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsPublic && t.IsConcrete())
                .Where(concreteType => concreteType.Closes(openGenericType))
                .ToArray();

            foreach (var implementationType in types)
            {
                builder.Add(new ExtendedServiceDescriptor(implementationType, implementationType, serviceLifetime, GetType()));

                var interfaces = implementationType.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    builder.Add(new ExtendedServiceDescriptor(@interface, context => context.GetService(implementationType), serviceLifetime,GetType()));
                }
            }

            return builder;
        }

        public IServiceCollection Register(IServiceCollection builder)
        {
            RegisterTypes(typeof(IPipelineBehavior<,>), builder, ServiceLifetime.Singleton);
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
