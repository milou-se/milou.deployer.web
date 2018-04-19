using System.Threading;
using Autofac;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    internal class LifetimeModule : Module
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public LifetimeModule(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => _cancellationTokenSource).AsSelf().SingleInstance();
        }
    }
}