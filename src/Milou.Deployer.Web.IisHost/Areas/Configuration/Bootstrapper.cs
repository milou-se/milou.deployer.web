using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    public static class Bootstrapper
    {
        public static IContainer Start(string basePath)
        {
            var builder = new ContainerBuilder();

            builder.Register(context => new ApplicationEnvironment(basePath)).AsSelf().SingleInstance();

            Type[] modules = AppDomain.CurrentDomain.FilteredAssemblies()
                .Select(assembly =>
                    assembly.GetLoadableTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsAssignableTo<IModule>()))
                .SelectMany(types => types)
                .ToArray();

            IContainer container = builder.Build();

            int GetOrder(Type type)
            {
                if (type.GetCustomAttribute(typeof(OrderAttribute)) is OrderAttribute orderAttribute)
                {
                    return orderAttribute.Order;
                }

                return int.MinValue;
            }

            IEnumerable<Type> moduleTypes = modules
                .Where(type => type.IsPublicClassWithDefaultConstructor())
                .Select(type => new ValueTuple<Type, int>(type, GetOrder(type)))
                .OrderBy(pair => pair.Item2)
                .Select(item => item.Item1);

            foreach (Type moduleType in moduleTypes)
            {
                if (Activator.CreateInstance(moduleType) is IModule module)
                {
                    module.Configure(container.ComponentRegistry);
                }
            }

            return container;
        }
    }
}