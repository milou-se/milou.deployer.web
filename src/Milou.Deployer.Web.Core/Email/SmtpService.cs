using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MailKit.Net.Smtp;
using MimeKit;
using Serilog;

namespace Milou.Deployer.Web.Core.Email
{
    [UsedImplicitly]
    public class SmtpService : ISmtpService
    {
        [NotNull]
        private readonly EmailConfiguration _emailConfiguration;

        private readonly ILogger _logger;

        public SmtpService(EmailConfiguration emailConfiguration, [NotNull] ILogger logger)
        {
            _emailConfiguration = emailConfiguration ?? new EmailConfiguration(
                                      null,
                                      null,
                                      -1,
                                      false,
                                      null,
                                      null,
                                      false);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendAsync([NotNull] MimeMessage mimeMessage, CancellationToken cancellationToken)
        {
            if (!_emailConfiguration.IsValid)
            {
                _logger.Warning("Email configuration is invalid {Configuration}", _emailConfiguration);
                return;
            }

            if (!_emailConfiguration.EmailEnabled)
            {
                _logger.Warning("Email configuration is disabled {Configuration}", _emailConfiguration);
                return;
            }

            if (mimeMessage == null)
            {
                throw new ArgumentNullException(nameof(mimeMessage));
            }

            if (!mimeMessage.From.Any())
            {
                mimeMessage.From.Add(new MailboxAddress(_emailConfiguration.DefaultFromEmailAddress));
            }

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailConfiguration.SmtpHost,
                    _emailConfiguration.Port,
                    _emailConfiguration.UseSsl,
                    cancellationToken);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                if (!string.IsNullOrWhiteSpace(_emailConfiguration.Username) &&
                    !string.IsNullOrWhiteSpace(_emailConfiguration.Password))
                {
                    await client.AuthenticateAsync(_emailConfiguration.Username,
                        _emailConfiguration.Password,
                        cancellationToken);
                }

                await client.SendAsync(mimeMessage, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}