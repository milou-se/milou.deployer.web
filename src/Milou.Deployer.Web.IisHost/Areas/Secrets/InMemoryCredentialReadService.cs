using System.Threading;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Credentials;

namespace Milou.Deployer.Web.IisHost.Areas.Secrets
{
    [UsedImplicitly]
    public class InMemoryCredentialReadService : ICredentialReadService
    {
        public string? GetSecret(string id, string secretKey, CancellationToken cancellationToken = default) => null;
    }
}