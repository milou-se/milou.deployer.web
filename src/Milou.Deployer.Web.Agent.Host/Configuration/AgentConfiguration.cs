using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Agent.Host.Configuration
{
    [UsedImplicitly]
    [Urn(Urn)]
    [Optional]
    public class AgentConfiguration : IConfigurationValues, IValidatableObject
    {
        public const string Urn = "urn:milou:deployer:web:agent:agent-config";

        public AgentConfiguration(string accessToken, string serverBaseUri, TimeSpan? startupDelay = default, bool? checkCertificateEnabled = true)
        {
            AccessToken = accessToken;
            ServerBaseUri = serverBaseUri;
            StartupDelay = startupDelay;
            CheckCertificateEnabled = checkCertificateEnabled;
        }

        public string AccessToken { get; }

        public string ServerBaseUri { get; }

        public bool? CheckCertificateEnabled { get; }

        public TimeSpan? StartupDelay { get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(ServerBaseUri))
            {
                yield return new ValidationResult($"{nameof(ServerBaseUri)} not defined");
            }

            if (!Uri.TryCreate(ServerBaseUri, UriKind.Absolute, out _))
            {
                yield return new ValidationResult($"{nameof(ServerBaseUri)} is invalid");
            }
        }
    }
}