using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Serilog;
using Module = Autofac.Module;

namespace Milou.Deployer.Web.IisHost.Areas.Messaging
{
    [UsedImplicitly]
    public class MediatorModule : Module
    {
        private readonly IReadOnlyCollection<Assembly> _scanAssemblies;
        private readonly IReadOnlyCollection<Type> _excludedTypes;
        private readonly ILogger _logger;

        public MediatorModule(IReadOnlyCollection<Assembly> scanAssemblies, IReadOnlyCollection<Type> excludedTypes, ILogger logger)
        {
            _scanAssemblies = scanAssemblies;
            _excludedTypes = excludedTypes;
            _logger = logger;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();

            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            Type[] mediatorOpenTypes =
            {
                typeof(IRequestHandler<,>),
                typeof(IRequestHandler<>),
                typeof(INotificationHandler<>)
            };

            foreach (Type mediatorOpenType in mediatorOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(_scanAssemblies.ToArray())
                    .Where(type => !_excludedTypes.Contains(type))
                    .Where(type =>
                    {
                        bool isClosedType = mediatorOpenTypes.Any(type.IsClosedTypeOf);

                        if (isClosedType)
                        {
                            _logger.Verbose("Registering closed type {Type}", type.FullName);
                        }

                        return isClosedType;
                    })
                    .AsClosedTypesOf(mediatorOpenType)
                    .AsImplementedInterfaces();
            }

            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}