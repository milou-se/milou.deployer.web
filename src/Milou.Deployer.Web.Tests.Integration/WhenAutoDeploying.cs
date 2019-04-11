using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.JsonConfiguration;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Extensions;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class WhenAutoDeploying : TestBase<AutoDeploySetup>
    {
        public WhenAutoDeploying(
            ITestOutputHelper output,
            AutoDeploySetup webFixture) : base(webFixture, output)
        {
        }


        [Fact(Skip = "Issues with postgresql permissions")]
        public async Task ThenNewVersionShouldBeDeployed()
        {
            SemanticVersion semanticVersion = null;

            var timeout = TimeSpan.FromSeconds(120);

            var expectedVersion = new SemanticVersion(1, 2, 5);

            if (WebFixture is null)
            {
                throw new DeployerAppException($"{nameof(WebFixture)} is null");
            }

            if (WebFixture.TestSiteHttpPort is null)
            {
                throw new DeployerAppException($"{nameof(WebFixture.TestSiteHttpPort)} is null");
            }

            if (WebFixture is null)
            {
                throw new DeployerAppException($"{nameof(WebFixture)} is null");
            }

            using (var httpClient = new HttpClient())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(timeout))
                {
                    while (!cancellationTokenSource.Token.IsCancellationRequested
                           && semanticVersion != expectedVersion)
                    {
                        var url = $"http://localhost:{WebFixture.TestSiteHttpPort.Port}/applicationmetadata.json";
                        string json;
                        try
                        {
                            using (var responseMessage = await httpClient.GetAsync(
                                url,
                                cancellationTokenSource.Token))
                            {
                                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

                                json = await responseMessage.Content.ReadAsStringAsync();
                            }
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            throw new DeployerAppException($"Could not get a valid response from request to '{url}'",
                                ex);
                        }

                        var tempFileName = Path.GetTempFileName();
                        await File.WriteAllTextAsync(tempFileName, json, Encoding.UTF8, cancellationTokenSource.Token);

                        var jsonKeyValueConfiguration =
                            new JsonKeyValueConfiguration(tempFileName);

                        if (File.Exists(tempFileName))
                        {
                            File.Delete(tempFileName);
                        }

                        var actual = jsonKeyValueConfiguration["urn:versioning:semver2:normalized"];

                        semanticVersion = SemanticVersion.Parse(actual);
                        // ReSharper disable once MethodSupportsCancellation
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            }

            Assert.Equal(expectedVersion, semanticVersion);
        }
    }
}
