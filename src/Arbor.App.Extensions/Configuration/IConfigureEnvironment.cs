using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.Core.Configuration
{
    public interface IConfigureEnvironment
    {
        void Configure(EnvironmentConfiguration environmentConfiguration);
    }
}
