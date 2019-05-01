//using System;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using JetBrains.Annotations;
//using Milou.Deployer.Web.Core.Targets;

//namespace Milou.Deployer.Web.Core.Deployment
//{
//    [UsedImplicitly]
//    public class JsonDeploymentTargetReadService : IDeploymentTargetReadService
//    {
//        private readonly JsonTargetSource _targetSource;

//        public JsonDeploymentTargetReadService([NotNull] JsonTargetSource targetSource)
//        {
//            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
//        }

//        public async Task<DeploymentTarget> GetDeploymentTargetAsync(
//            string deploymentTargetId,
//            CancellationToken cancellationToken = default)
//        {
//            var organizations = await _targetSource.GetTargetsAsync(cancellationToken);

//            return organizations.SelectMany(org => org.Projects.SelectMany(project => project.DeploymentTargets))
//                .SingleOrDefault(target => target.Id.Equals(deploymentTargetId, StringComparison.OrdinalIgnoreCase));
//        }

//        public async Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(
//            CancellationToken cancellationToken = default)
//        {
//            var organizations = await _targetSource.GetTargetsAsync(cancellationToken);

//            return organizations.ToImmutableArray();
//        }

//        public Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(CancellationToken stoppingToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(
//            string organizationId,
//            CancellationToken cancellationToken = default)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
