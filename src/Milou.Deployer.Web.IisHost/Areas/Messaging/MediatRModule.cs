using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Module = Autofac.Module;

namespace Milou.Deployer.Web.IisHost.Areas.Messaging
{
    [UsedImplicitly]
    public class MediatRModule : Module
    {
        private readonly IReadOnlyCollection<Assembly> _scanAssemblies;

        public MediatRModule(IReadOnlyCollection<Assembly> scanAssemblies)
        {
            _scanAssemblies = scanAssemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();

            builder.Register<SingleInstanceFactory>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return type =>
                {
                    object returnValue = context.TryResolve(type, out object instance) ? instance : null;

                    return returnValue;
                };
            });

            builder.Register<MultiInstanceFactory>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return type =>
                {
                    var enumerable = (IEnumerable<object>)context.Resolve(typeof(IEnumerable<>).MakeGenericType(type));

                    return enumerable;
                };
            });

            Type[] mediatrOpenTypes =
            {
                typeof(IRequestHandler<,>),
                typeof(IRequestHandler<>),
                typeof(INotificationHandler<>)
            };

            foreach (Type mediatrOpenType in mediatrOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(_scanAssemblies.ToArray())
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();
            }

            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}