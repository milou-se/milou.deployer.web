using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Protocols.Http;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [UsedImplicitly]
    public class NuGetService
    {
        private readonly HttpClientFactory _httpClientFactory;

        public NuGetService(HttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task ClearAsync()
        {
            string baseUrlValue =
                StaticKeyValueConfigurationManager.AppSettings["milou-deployer-web:nuget-server:cleanup:base-url"]
                    .ThrowIfEmpty(
                        "base-url");
            string username =
                StaticKeyValueConfigurationManager.AppSettings["milou-deployer-web:nuget-server:cleanup:username"]
                    .ThrowIfEmpty(
                        "username");
            string password =
                StaticKeyValueConfigurationManager.AppSettings["milou-deployer-web:nuget-server:cleanup:password"]
                    .ThrowIfEmpty(
                        "password");

            HttpClient restClient = _httpClientFactory.GetHttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrlValue}/api/command")
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        command = "del *.bin",
                        dir = @"site\wwwroot\app_data\nuget\packages\"
                    }),
                    Encoding.UTF8,
                    "application/json")
            };


            HttpResponseMessage response = await restClient.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Could not cleanup nuget server cache");
            }
        }
    }
}