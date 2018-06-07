using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [Urn(WorkerConstants.Configuration)]
    [UsedImplicitly]
    public class WorkerConfiguration
    {
        public WorkerConfiguration(int messageTimeOutInSeconds)
        {
            MessageTimeOutInSeconds = messageTimeOutInSeconds;
        }

        public int MessageTimeOutInSeconds { get; }
    }
}