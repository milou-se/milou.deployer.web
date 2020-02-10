﻿using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;

namespace Milou.Deployer.Web.Core.Security
{
    [Optional]
    [Urn(UrnKey)]
    public class AllowedEmailDomain : IConfigurationValues
    {
        public const string UrnKey = "urn:milou:deployer:web:allowed-email-domain";

        public AllowedEmailDomain(string domain)
        {
            Domain = domain;
            IsValid = !string.IsNullOrWhiteSpace(domain);
        }

        public string Domain { get; }

        public bool IsValid { get; }
    }
}
