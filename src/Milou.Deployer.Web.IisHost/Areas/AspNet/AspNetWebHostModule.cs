using System;
using System.Threading;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;

namespace Milou.Deployer.Web.IisHost.Areas.AspNet
{
    public class AspNetWebHostModule : Module
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IComponentContext _componentContext;

        public AspNetWebHostModule(
            [NotNull] CancellationTokenSource cancellationTokenSource,
            [NotNull] IComponentContext componentContext)
        {
            _cancellationTokenSource = cancellationTokenSource ??
                                       throw new ArgumentNullException(nameof(cancellationTokenSource));
            _componentContext = componentContext ?? throw new ArgumentNullException(nameof(componentContext));
        }

        protected override void Load(ContainerBuilder builder)
        {
            Serilog.Log.Logger.Debug("Running module {Module}", nameof(AspNetWebHostModule));

            IWebHostBuilder webHostBuilder = CustomWebHostBuilder.GetWebHostBuilder(_componentContext);

            builder
                .Register(context => _cancellationTokenSource)
                .AsSelf()
                .SingleInstance();

            builder
                .Register(context => _cancellationTokenSource.Token)
                .AsSelf()
                .SingleInstance();

            builder
                .Register(context => webHostBuilder)
                .SingleInstance()
                .AsSelf()
                .AsImplementedInterfaces();
        }
    }
}