﻿using System;
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
        public async Task ThenNewVersionShouldBeDeployed()
        {
            SemanticVersion semanticVersion = null;

            TimeSpan timeout = TimeSpan.FromSeconds(80);

            var expectedVersion = new SemanticVersion(1,2,5);

            if (WebFixture is null)
            {
                throw new InvalidOperationException($"{nameof(WebFixture)} is null");
            }

            if (WebFixture.TestSiteHttpPort is null)
            {
                throw new InvalidOperationException($"{nameof(WebFixture.TestSiteHttpPort)} is null");
            }

            if (WebFixture is null)
            {
                throw new InvalidOperationException($"{nameof(WebFixture)} is null");
            }

            using (var httpClient = new HttpClient())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(timeout))
                {
                    while (!cancellationTokenSource.IsCancellationRequested &&
                           semanticVersion != expectedVersion)
                    {
                        string url = $"http://localhost:{WebFixture.TestSiteHttpPort.Port}/applicationmetadata.json";
                        string json;
                        try
                        {
                            using (HttpResponseMessage responseMessage = await httpClient.GetAsync(
                                url,
                                cancellationTokenSource.Token))
                            {
                                Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

                                json = await responseMessage.Content.ReadAsStringAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Could not get a valid response from request to '{url}'", ex);
                        }

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