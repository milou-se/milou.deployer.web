using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Marten;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Serilog;

namespace Milou.Deployer.Web.Marten
{
    [UsedImplicitly]
    public class MartenDeploymentFinishedLogHandler : INotificationHandler<DeploymentFinishedNotification>
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger _logger;

        public MartenDeploymentFinishedLogHandler([NotNull] IDocumentStore documentStore, ILogger logger)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logger = logger;
        }


        public async Task Handle(DeploymentFinishedNotification notification, CancellationToken cancellationToken)
        {
            string taskLogId = $"deploymentTaskLog/{notification.DeploymentTask.DeploymentTaskId}";

            using (var session = _documentStore.OpenSession())
            {
                var existing = await session.Query<TaskLog>().Where(taskLog => taskLog.Id == taskLogId).ToListAsync();

                if (existing.Any())
                {
                    _logger.Warning("There is already a task log with id {TaskLogId}", taskLogId);
                    return;
                }

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

            try
            {
                _documentStore.BulkInsert(notification.LogLines);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not bulk insert log lines");
            }
        }
    }
}
