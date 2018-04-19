using System.Threading;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Secrets
{
    public class InMemoryCredentialReadService : ICredentialReadService
    {
        public string GetSecretAsync(string id, string secretKey, CancellationToken cancellationToken = default)
        {

            return null;
        }
    }
}