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
    public class MartenDeploymentFinishedLogHandler : INotificationHandler<DeploymentFinishedNotification>
    {
        private readonly IDocumentStore _documentStore;

        public MartenDeploymentFinishedLogHandler([NotNull] IDocumentStore documentStore)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task Handle(DeploymentFinishedNotification notification, CancellationToken cancellationToken)
        {
            using (var session = _documentStore.OpenSession())
            {
                var taskMetadata = new TaskLog
                {
                    DeploymentTaskId = notification.DeploymentTask.DeploymentTaskId,
                    DeploymentTargetId = notification.DeploymentTask.DeploymentTargetId,
                    Id = $"deploymentTaskLog/{notification.DeploymentTask.DeploymentTaskId}",
                    Log = notification.Log,
                    FinishedAtUtc = notification.FinishedAtUtc
                };

                session.Store(taskMetadata);

                await session.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
