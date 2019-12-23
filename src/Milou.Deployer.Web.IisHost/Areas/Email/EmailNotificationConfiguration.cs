using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Configuration;
using Arbor.App.Extensions.Validation;
using Arbor.KVConfiguration.Core.Metadata;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Email
{
    [Urn(EmailNotificationConfigurationKey)]
    [UsedImplicitly]
    public class EmailNotificationConfiguration : IValidationObject, IConfigurationValues
    {
        [PublicAPI]
        [Metadata(defaultValue: "false")]
        public const string EmailNotificationConfigurationDefaultEnabled =
            "urn:milou:deployer:web:email:notifications:default:enabled";

        [PublicAPI]
        [Metadata]
        public const string EmailNotificationConfigurationKey = "urn:milou:deployer:web:email:notifications";

        public EmailNotificationConfiguration(
            bool enabled,
            IEnumerable<EmailAddress> to,
            EmailAddress from)
        {
            Enabled = enabled;
            To = to.SafeToImmutableArray();
            From = from;
        }

        public bool Enabled { get; }

        public ImmutableArray<EmailAddress> To { get; }

        public EmailAddress From { get; }

        public override string ToString()
        {
            return
                $"{nameof(Enabled)}: {Enabled}, {nameof(To)}: {string.Join("; ", To.Select(email => email.Address))}, {nameof(From)}: {From}, {nameof(IsValid)}: {IsValid}";
        }

        public bool IsValid => !Enabled
                               || !To.IsDefaultOrEmpty
                               && To.All(s => s.IsValid)
                               && From?.IsValid == true;
    }
}
