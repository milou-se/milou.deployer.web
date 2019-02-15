using System;
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
            Type openType = typeof(INotificationHandler<>);

            Type closedType = typeof(DeploymentHubLogHandler);

            Assert.True(closedType.IsClosedTypeOf(openType) && closedType.IsPublic && !closedType.IsAbstract);
        }
    }
}