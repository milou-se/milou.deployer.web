using Autofac;
using MediatR;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class TypeCheck
    {
        [Fact]
        public void Do()
        {
            var openType = typeof(INotificationHandler<>);

            var closedType = typeof(DeploymentHubLogHandler);

            Assert.True(closedType.IsClosedTypeOf(openType) && closedType.IsPublic && !closedType.IsAbstract);
        }
    }
}
