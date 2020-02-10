using System;
using Arbor.AspNetCore.Host;
using Milou.Deployer.Web.IisHost.AspNetCore.Startup;

namespace Milou.Deployer.Web.Tests.Integration
{
    public interface IAppHost
    {
        App<ApplicationPipeline> App { get; }

        Exception Exception { get; }
    }
}
