using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        [Fact]
        public async Task Abc()
        {
            SemanticVersion semanticVersion = null;

            TimeSpan timeout = TimeSpan.FromSeconds(30);

            var expectedVersion = new SemanticVersion(1,2,5);

            using (var httpClient = new HttpClient())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(timeout))
                {
                    while (!cancellationTokenSource.IsCancellationRequested &&
                           semanticVersion != expectedVersion)
                    {
                        HttpResponseMessage responseMessage = await httpClient.GetAsync(
                            $"http://localhost:{WebFixture.TestSiteHttpPort}/applicationmetadata.json",
                            cancellationTokenSource.Token);

                        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

                        string json = await responseMessage.Content.ReadAsStringAsync();

                        string tempFileName = Path.GetTempFileName();
                        await File.WriteAllTextAsync(tempFileName, json, Encoding.UTF8, cancellationTokenSource.Token);

                        var jsonKeyValueConfiguration =
                            new Arbor.KVConfiguration.JsonConfiguration.JsonKeyValueConfiguration(tempFileName);

                        if (File.Exists(tempFileName))
                        {
                            File.Delete(tempFileName);
                        }

                        string actual = jsonKeyValueConfiguration["urn:versioning:semver2:normalized"];

                        semanticVersion = SemanticVersion.Parse(actual);
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
                    }
                }
            }

            Assert.Equal(expectedVersion, semanticVersion);
        }
    }
}