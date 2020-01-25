using System;
using System.Linq;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.Configuration;
using JetBrains.Annotations;

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
