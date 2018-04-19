using System.Threading;

namespace Milou.Deployer.Web.Core.Deployment
{
    public interface ICredentialReadService
    {
        string GetSecretAsync(string id, string secretKey, CancellationToken cancellationToken = default);
    }
}