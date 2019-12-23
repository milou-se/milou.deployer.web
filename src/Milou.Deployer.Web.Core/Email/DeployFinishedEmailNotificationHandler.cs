using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Time;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Extensions;
using MimeKit;
using Serilog;

namespace Milou.Deployer.Web.Core.Email
{
    [UsedImplicitly]
    public class DeployFinishedEmailNotificationHandler : INotificationHandler<DeploymentMetadataLogNotification>
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger _logger;
        private readonly ISmtpService _smtpService;
        private readonly IDeploymentTargetReadService _targetSource;
        private readonly TimeoutHelper _timeoutHelper;

        public DeployFinishedEmailNotificationHandler(
            [NotNull] ISmtpService smtpService,
            [NotNull] IDeploymentTargetReadService targetSource,
            [NotNull] ILogger logger,
            TimeoutHelper timeoutHelper,
            EmailConfiguration emailConfiguration = null)
        {
            _smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeoutHelper = timeoutHelper;

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

        public async Task Handle(DeploymentMetadataLogNotification notification, CancellationToken cancellationToken)
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

            using (var cancellationTokenSource =
                _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(_emailConfiguration.NotificationTimeOutInSeconds)))
            {
                var target =
                    await _targetSource.GetDeploymentTargetAsync(notification.DeploymentTask.DeploymentTargetId,
                        cancellationTokenSource.Token);

                if (target is null)
                {
                    return;
                }

                if (!target.EmailNotificationAddresses.Any())
                {
                    return;
                }

                var mimeMessage = new MimeMessage();

                foreach (var targetEmailNotificationAddress in target.EmailNotificationAddresses)
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
{notification.Result.Metadata}"
                };

                mimeMessage.Subject = $"Deployment finished for {notification.DeploymentTask}";

                try
                {
                    await _smtpService.SendAsync(mimeMessage, cancellationTokenSource.Token);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Error(ex,
                        "Could not send email to '{To}'",
                        string.Join(", ", target.EmailNotificationAddresses));
                }
            }
        }
    }
}
