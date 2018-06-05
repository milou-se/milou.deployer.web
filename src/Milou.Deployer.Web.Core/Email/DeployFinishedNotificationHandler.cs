using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Structure;
using MimeKit;
using Serilog;

namespace Milou.Deployer.Web.Core.Email
{
    [UsedImplicitly]
    public class DeployFinishedNotificationHandler : INotificationHandler<DeploymentFinishedNotification>
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger _logger;
        private readonly ISmtpService _smtpService;
        private readonly IDeploymentTargetReadService _targetSource;

        public DeployFinishedNotificationHandler(
            [NotNull] ISmtpService smtpService,
            [NotNull] IDeploymentTargetReadService targetSource,
            [NotNull] ILogger logger,
            EmailConfiguration emailConfiguration = null)
        {
            _smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _emailConfiguration = emailConfiguration ?? new EmailConfiguration(
                                      null,
                                      null,
                                      -1,
                                      false,
                                      null,
                                      null,
                                      30,
                                      false);
        }

        public async Task Handle(DeploymentFinishedNotification notification, CancellationToken cancellationToken)
        {
            if (!_emailConfiguration.IsValid)
            {
                _logger.Warning("Email configuration is invalid {Configuration}", _emailConfiguration);
                return;
            }

            if (!_emailConfiguration.EmailEnabled)
            {
                _logger.Debug(
                    "Email is disabled, skipping sending deployment finished email for notification {Notification}",
                    notification);
                return;
            }

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_emailConfiguration.NotificationTimeOutInSeconds));

            IReadOnlyCollection<OrganizationInfo> targets =
                await _targetSource.GetOrganizationsAsync(cancellationTokenSource.Token);

            DeploymentTarget target = targets.SelectMany(o => o.Projects)
                .SelectMany(project => project.DeploymentTargets)
                .SingleOrDefault(deploymentTarget =>
                    deploymentTarget.Id == notification.DeploymentTask.DeploymentTargetId);

            if (target is null)
            {
                return;
            }

            if (!target.EmailNotificationAddresses.Any())
            {
                return;
            }

            var mimeMessage = new MimeMessage();

            foreach (string targetEmailNotificationAddress in target.EmailNotificationAddresses)
            {
                try
                {
                    mimeMessage.To.Add(new MailboxAddress(targetEmailNotificationAddress));
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Error(ex,
                        "Could not add email address {EmailAddress} when sending deployment finished notification email",
                        targetEmailNotificationAddress);
                }
            }

            mimeMessage.Body = new TextPart
            {
                Text =
                    $@"Deployment finished for {notification.DeploymentTask}
{notification.MetadataContent}"
            };

            mimeMessage.Subject = $"Deployment finished for {notification.DeploymentTask}";

            try
            {
                await _smtpService.SendAsync(mimeMessage, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Could not send email to '{To}'",
                    string.Join(", ", target.EmailNotificationAddresses));
            }
        }
    }
}