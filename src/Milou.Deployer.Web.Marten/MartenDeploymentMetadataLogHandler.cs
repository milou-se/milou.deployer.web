using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Marten;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.Marten
{
    [UsedImplicitly]
    public class MartenDeploymentMetadataLogHandler : INotificationHandler<DeploymentMetadataLogNotification>
    {
        private readonly IDocumentStore _documentStore;

        public MartenDeploymentMetadataLogHandler([NotNull] IDocumentStore documentStore)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task Handle(DeploymentMetadataLogNotification notification, CancellationToken cancellationToken)
        {
            using (var session = _documentStore.OpenSession())
            {
                var taskMetadata = new TaskMetadata
                {
                    PackageId = notification.DeploymentTask.PackageId,
                    Version = notification.DeploymentTask.SemanticVersion.ToNormalizedString(),
                    DeploymentTaskId = notification.DeploymentTask.DeploymentTaskId,
                    DeploymentTargetId = notification.DeploymentTask.DeploymentTargetId,
                    Id = $"deploymentTaskMetadata/{notification.DeploymentTask.DeploymentTaskId}",
                    StartedAtUtc = notification.Result.StartedAtUtc,
                    FinishedAtUtc = notification.Result.FinishedAtUtc,
                    Metadata = notification.Result.Metadata,
                    ExitCode = notification.Result.ExitCode.Code
                };

                session.Store(taskMetadata);

                await session.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
