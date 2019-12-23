﻿using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Security
{
    [Urn(HostNameUrn)]
    [Optional]
    [UsedImplicitly]
    public class AllowedHostName : IConfigurationValues
    {
        [PublicAPI]
        public const string HostNameUrn = "urn:milou:deployer:web:allowed-host";

        public AllowedHostName(string hostName)
        {
            HostName = hostName;
        }

        public string HostName { get; }

        public override string ToString()
        {
            return $"{nameof(HostName)}: {HostName}";
        }
    }
}
