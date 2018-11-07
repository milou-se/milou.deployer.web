using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.KVConfiguration.Core.Metadata;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Validation;

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
            IEnumerable<Email> to,
            Email from)
        {
            Enabled = enabled;
            To = to.SafeToImmutableArray();
            From = from;
        }

        public bool Enabled { get; }

        public ImmutableArray<Email> To { get; }

        public Email From { get; }

        public bool IsValid => !Enabled
                               || (!To.IsDefaultOrEmpty
                                   && To.All(s => s.IsValid)
                                   && From?.IsValid == true);

        public override string ToString()
        {
            return $"{nameof(Enabled)}: {Enabled}, {nameof(To)}: {To}, {nameof(From)}: {From}, {nameof(IsValid)}: {IsValid}";
        }
    }
}