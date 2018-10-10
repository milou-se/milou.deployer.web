using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
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

            var message = new MimeMessage
            {
                Body = new TextPart("plain")
                {
                    Text = $@"{notification.DeploymentTask}"
                },
                Subject = $"Deployment result for {notification.DeploymentTask.DeploymentTargetId}"
            };


            foreach (Email email in _emailNotificationConfiguration.To)
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