using System;
using System.Linq;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Development
{
    [UsedImplicitly]
    public class DevelopmentModeConfigurator : IConfigureEnvironment
    {
        public void Configure(EnvironmentConfiguration environmentConfiguration)
        {
            if (environmentConfiguration.CommandLineArgs.Any(arg =>
                arg.Equals(ApplicationConstants.DevelopmentMode, StringComparison.OrdinalIgnoreCase)))
            {
                environmentConfiguration.UseVerboseLogging = true;
                environmentConfiguration.HttpPort = 34345;
                environmentConfiguration.IsDevelopmentMode = true;
            }
        }
    }
}
