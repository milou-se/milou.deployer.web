using Arbor.App.Extensions.Application;

namespace Arbor.App.Extensions.Configuration
{
    public interface IConfigureEnvironment
    {
        void Configure(EnvironmentConfiguration environmentConfiguration);
    }
}