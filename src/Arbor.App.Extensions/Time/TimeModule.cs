using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.App.Extensions.Time
{
    [UsedImplicitly]
    public class TimeModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) =>
            builder
                .AddSingleton<ICustomClock, CustomSystemClock>(this)
                .AddSingleton<TimeoutHelper>(this);
    }
}