using System.Net.Http;
using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class TestHttpClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new HttpClient());
        }
    }
}