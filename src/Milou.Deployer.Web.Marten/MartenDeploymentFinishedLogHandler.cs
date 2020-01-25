using System;
using System.Linq;
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
            string taskLogId = $"deploymentTaskLog/{notification.DeploymentTask.DeploymentTaskId}";

            using (var session = _documentStore.OpenSession())
            {
                var taskMetadata = new TaskLog
                {
                    DeploymentTaskId = notification.DeploymentTask.DeploymentTaskId,
                    DeploymentTargetId = notification.DeploymentTask.DeploymentTargetId,
                    Id = taskLogId,
                    FinishedAtUtc = notification.FinishedAtUtc
                };

                session.Store(taskMetadata);

                await session.SaveChangesAsync(cancellationToken);
            }

            foreach (var notificationLogLine in notification.LogLines.Select((item, index) => (item,index)))
            {
                notificationLogLine.item.TaskLogId = taskLogId;
                notificationLogLine.item.Id = $"{taskLogId}/{notificationLogLine.index + 1}";
            }

            _documentStore.BulkInsert(notification.LogLines);
        }
    }
}
