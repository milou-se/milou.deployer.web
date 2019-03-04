using System.Threading;

namespace Milou.Deployer.Web.Core.Deployment
{
    public interface ICredentialReadService
    {
        string GetSecret(string id, string secretKey, CancellationToken cancellationToken = default);
    }
}
