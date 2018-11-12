using System;
using System.Reflection;
using Autofac.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public class ModuleRegistration : IModuleRegistration
    {
        [NotNull]
        public Type ModuleType { get; }

        public ModuleRegistration([NotNull] Type moduleType)
        {
            ModuleType = moduleType ?? throw new ArgumentNullException(nameof(moduleType));

            Type interfaceType = typeof(IModule);

            if (!interfaceType.IsAssignableFrom(moduleType))
            {
                throw new ArgumentException($"Type {moduleType.FullName} is not an instance of {interfaceType.FullName}", nameof(moduleType));
            }

            if (moduleType.IsAbstract)
            {
                throw new ArgumentException($"The type {moduleType.FullName} is abstract ");
            }

            var registrationOrderAttribute = ModuleType.GetCustomAttribute<RegistrationOrderAttribute>();

            Order = registrationOrderAttribute?.Order ?? 0;

            Tag = registrationOrderAttribute?.Tag;

            RegisterInRootScope = registrationOrderAttribute?.RegisterInRootScope ?? false;
        }

        public string Tag { get; }

        public int Order { get; }

        public bool RegisterInRootScope { get; }
    }
}