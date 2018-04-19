using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Email
{
    [Urn(ConfigurationConstants.EmailConfiguration)]
    [UsedImplicitly]
    public class EmailConfiguration : IValidationObject
    {
        public EmailConfiguration(
            string defaultFromEmailAddress,
            string smtpHost,
            int port,
            bool useSsl,
            string username,
            string password,
            bool emailEnabled = true)
        {
            DefaultFromEmailAddress = defaultFromEmailAddress;
            SmtpHost = smtpHost;
            Port = port;
            UseSsl = useSsl;
            Username = username;
            Password = password;
            EmailEnabled = emailEnabled;
        }

        public string DefaultFromEmailAddress { get; }

        public string SmtpHost { get; }

        public int Port { get; }

        public bool UseSsl { get; }

        public string Username { get; }

        public string Password { get; }

        public bool EmailEnabled { get; }

        public bool IsValid =>
            !EmailEnabled || (SmtpHost.HasValue() && Port >= 0 && DefaultFromEmailAddress.HasValue());

        public override string ToString()
        {
            return $"{nameof(DefaultFromEmailAddress)}: {DefaultFromEmailAddress}, {nameof(SmtpHost)}: {SmtpHost}, {nameof(Port)}: {Port}, {nameof(UseSsl)}: {UseSsl}, {nameof(Username)}: {Username}, {nameof(Password)}: ******, {nameof(EmailEnabled)}: {EmailEnabled}, {nameof(IsValid)}: {IsValid}";
        }
    }
}