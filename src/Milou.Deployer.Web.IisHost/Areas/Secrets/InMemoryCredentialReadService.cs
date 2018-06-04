using System.Threading;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Secrets
{
    [UsedImplicitly]
    public class InMemoryCredentialReadService : ICredentialReadService
    {
        public string GetSecretAsync(string id, string secretKey, CancellationToken cancellationToken = default)
        {

            return null;
        }
    }
}