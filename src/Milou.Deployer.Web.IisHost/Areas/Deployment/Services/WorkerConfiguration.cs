using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [Urn(WorkerConstants.Configuration)]
    [UsedImplicitly]
    public class WorkerConfiguration : IConfigurationValues
    {
        public WorkerConfiguration(int messageTimeOutInSeconds)
        {
            MessageTimeOutInSeconds = messageTimeOutInSeconds;
        }

        public int MessageTimeOutInSeconds { get; }
    }
}