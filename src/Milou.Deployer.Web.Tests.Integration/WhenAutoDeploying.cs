using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.KVConfiguration.JsonConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Startup;
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
                using (var cancellationTokenSource =
                    WebFixture.App.Host.Services.GetService<CancellationTokenSource>())
                {
                    cancellationTokenSource.Token.Register(() =>
                    {
                        Debug.WriteLine("Cancellation for app in test");
                    });

                    while (!cancellationTokenSource.Token.IsCancellationRequested
                           && semanticVersion != expectedVersion)
                    {
                        // ReSharper disable MethodSupportsCancellation
                        var startupTaskContext =
                            WebFixture.App.Host.Services.GetRequiredService<StartupTaskContext>();

                        while (!startupTaskContext.IsCompleted &&
                               !cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(50));
                        }

                        var url = new Uri($"http://localhost:{WebFixture.TestSiteHttpPort.Port.Port + 1}/applicationmetadata.json");

                        string contents;
                        try
                        {
                            using (var responseMessage = await httpClient.GetAsync(url))
                            {
                                contents = await responseMessage.Content.ReadAsStringAsync();

                                Output.WriteLine($"{responseMessage.StatusCode} {contents}");

                                if (responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable
                                    || responseMessage.StatusCode == HttpStatusCode.NotFound)
                                {
                                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                                    continue;
                                }

                                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
                            }
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            throw new DeployerAppException($"Could not get a valid response from request to '{url}'",
                                ex);
                        }

                        var tempFileName = Path.GetTempFileName();
                        await File.WriteAllTextAsync(tempFileName,
                            contents,
                            Encoding.UTF8,
                            cancellationTokenSource.Token);

                        var jsonKeyValueConfiguration =
                            new JsonKeyValueConfiguration(tempFileName);

                        if (File.Exists(tempFileName))
                        {
                            File.Delete(tempFileName);
                        }

                        var actual = jsonKeyValueConfiguration["urn:versioning:semver2:normalized"];

                        semanticVersion = SemanticVersion.Parse(actual);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        // ReSharper restore MethodSupportsCancellation
                    }
                }
            }

            Assert.Equal(expectedVersion, semanticVersion);
        }
    }
}
