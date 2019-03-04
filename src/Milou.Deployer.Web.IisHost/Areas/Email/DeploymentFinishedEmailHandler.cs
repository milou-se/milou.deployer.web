using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Email;
using MimeKit;

namespace Milou.Deployer.Web.IisHost.Areas.Email
{
    [UsedImplicitly]
    public class DeploymentFinishedEmailHandler : INotificationHandler<DeploymentFinishedNotification>
    {
        private readonly EmailNotificationConfiguration _emailNotificationConfiguration;
        private readonly ISmtpService _smtpService;

        public DeploymentFinishedEmailHandler(
            [NotNull] ISmtpService smtpService,
            [NotNull] EmailNotificationConfiguration emailNotificationConfiguration)
        {
            _smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
            _emailNotificationConfiguration = emailNotificationConfiguration ??
                                              throw new ArgumentNullException(nameof(emailNotificationConfiguration));
        }

        public Task Handle(DeploymentFinishedNotification notification, CancellationToken cancellationToken)
        {
            if (!_emailNotificationConfiguration.Enabled)
            {
                return Task.CompletedTask;
            }

            if (!_emailNotificationConfiguration.IsValid)
            {
                return Task.CompletedTask;
            }

            var result = notification.DeploymentTask.Status == WorkTaskStatus.Done ? "succeeded" : "failed";

            var subject =
                $"Deployment of {notification.DeploymentTask.PackageId} {notification.DeploymentTask.SemanticVersion.ToNormalizedString()} to {notification.DeploymentTask.DeploymentTargetId} {result}";

            var body = $@"{notification.DeploymentTask.DeploymentTargetId}
Status: {notification.DeploymentTask.Status}
Finished at time (UTC): {notification.FinishedAtUtc:O}
Package ID: {notification.DeploymentTask.PackageId}
Deployment task ID: {notification.DeploymentTask.DeploymentTaskId}
Version: {notification.DeploymentTask.SemanticVersion.ToNormalizedString()}
Log: {notification.Log}
";

            var message = new MimeMessage
            {
                Body = new TextPart("plain")
                {
                    Text = body
                },
                Subject = subject
            };

            foreach (var email in _emailNotificationConfiguration.To)
            {
                message.To.Add(new MailboxAddress(email.Address));
            }

            if (_emailNotificationConfiguration.From != null)
            {
                message.From.Add(new MailboxAddress(_emailNotificationConfiguration.From.Address));
            }

            return _smtpService.SendAsync(message, cancellationToken);
        }
    }
}
