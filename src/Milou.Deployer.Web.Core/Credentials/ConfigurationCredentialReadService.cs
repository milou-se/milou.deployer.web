using System;
using System.Threading;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Serilog;

namespace Milou.Deployer.Web.Core.Credentials
{
    [UsedImplicitly]
    public class ConfigurationCredentialReadService : ICredentialReadService
    {
        [NotNull]
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        private readonly ILogger _logger;

        public ConfigurationCredentialReadService(
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ILogger logger)
        {
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetSecret(
            [NotNull] string id,
            [NotNull] string secretKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(secretKey));
            }

            var combinedKey = $"{ConfigurationConstants.SecretsKeyPrefix}{id}:{secretKey}";

            var value = _keyValueConfiguration[combinedKey];

            var anonymous = string.IsNullOrWhiteSpace(value) ? Constants.NotAvailable : new string('*', value.Length);

            _logger.Debug(
                "Getting secret for target id {TargetId}, secret key {SecretKey}, combined key {CombinedKey}, value (anonymous) '{Value}'",
                id,
                secretKey,
                combinedKey,
                anonymous);

            return value;
        }
    }
}
