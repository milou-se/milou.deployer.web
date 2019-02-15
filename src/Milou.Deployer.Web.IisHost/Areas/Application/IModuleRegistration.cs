using System;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public interface IModuleRegistration
    {
        Type ModuleType { get; }

        string Tag { get; }

        int Order { get; }

        bool RegisterInRootScope { get; }
    }
}