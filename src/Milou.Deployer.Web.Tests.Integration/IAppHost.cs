using System;
using System.Text;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.Tests.Integration
{
    public interface IAppHost
    {
        App App { get; }

        Exception Exception { get; }

        StringBuilder Builder { get; }
    }
}
