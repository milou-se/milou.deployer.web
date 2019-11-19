using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;

using Milou.Deployer.Web.Core.Deployment.Packages;

using Newtonsoft.Json;

using NexusWebHook;

using NuGet.Versioning;

using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    [UsedImplicitly]
    public class NexusPackageWebHook : IPackageWebHook, IDisposable
    {
        private readonly HMACSHA1 _hmacSha1;

        private readonly ILogger _logger;

        private readonly NexusConfig _nexusConfig;

        public NexusPackageWebHook(NexusConfig nexusConfig, ILogger logger)
        {
            _nexusConfig = nexusConfig;
            _logger = logger;

            var key = Encoding.UTF8.GetBytes(nexusConfig.HmacKey);
            _hmacSha1 = new HMACSHA1(key);
        }

        public void Dispose() => _hmacSha1?.Dispose();

        public async Task<PackageWebHookNotification> TryGetWebHookNotification(HttpRequest request)
        {
            if (!request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("Web hook request is not json");
                return null;
            }

            if (!request.Headers.TryGetValue("X-Nexus-Webhook-Signature", out var signature))
            {
                _logger.Debug("Web hook request does not contain nexus signature header");
                return null;
            }

            using var streamReader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);

            string json = await streamReader.ReadToEndAsync();

            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var computedHash = _hmacSha1.ComputeHash(jsonBytes);
            var expectedBytes = ((string)signature).ToByteArray();

            if (!computedHash.SequenceEqual(expectedBytes))
            {
                _logger.Error("Nexus web hook signature validation failed");
                return null;
            }

            var webHookNotification = JsonConvert.DeserializeObject<NexusWebHookNotification>(json);

            if (string.IsNullOrWhiteSpace(webHookNotification?.Audit?.Attributes?.Name))
            {
                _logger.Debug("Nexus web hook notification does not contain audit attribute name");
                return null;
            }

            var split = webHookNotification.Audit.Attributes.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != 2)
            {
                _logger.Debug("Unexpected attribute name value '{Name}'", webHookNotification.Audit.Attributes.Name);
                return null;
            }

            string name = split[0];
            string version = split[1];

            if (!SemanticVersion.TryParse(version, out SemanticVersion semanticVersion))
            {
                _logger.Debug("Could not parse semantic version from Nexus web hook notification, '{Version}'", version);
                return null;
            }

            var packageVersion = new PackageVersion(name, semanticVersion);
            _logger.Information("Successfully received Nexus web hook notification for package {Package}", packageVersion);

            return new PackageWebHookNotification(packageVersion, _nexusConfig.NuGetSource, _nexusConfig.NuGetConfig);
        }
    }
}