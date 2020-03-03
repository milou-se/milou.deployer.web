using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Settings;
using Milou.Deployer.Web.Marten;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    [UsedImplicitly]
    public class SettingsModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) => builder.AddSingleton<IApplicationSettingsStore, MartenSettingsStore>();
    }
}