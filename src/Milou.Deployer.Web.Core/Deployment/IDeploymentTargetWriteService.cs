using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Deployment
{
    public interface IDeploymentTargetWriteService
    {
        Task CreateOrganizationAsync(CreateOrganization createOrganization, CancellationToken cancellationToken);

        Task CreateProjectAsync(CreateProject createProject, CancellationToken cancellationToken);
    }
}