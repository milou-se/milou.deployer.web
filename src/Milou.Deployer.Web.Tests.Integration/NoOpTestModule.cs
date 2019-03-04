using Arbor.KVConfiguration.Core;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Tests.Integration
{
    [RegistrationOrder(37, Tag = Scope.AspNetCoreScope, RegisterInRootScope = true)]
    [UsedImplicitly]
    public class NoOpTestModule : Module
    {
        public NoOpTestModule(IKeyValueConfiguration keyValueConfiguration)
        {
            MeaningOfLife = keyValueConfiguration[nameof(MeaningOfLife)];
        }

        public string MeaningOfLife { get; }

        protected override void Load(ContainerBuilder builder)
        {
        }
    }
}
