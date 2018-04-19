using System.Net.Http;

namespace Milou.Deployer.Web.IisHost.Areas.Protocols.Http
{
    public class HttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            var httpMessageHandler = new HttpClientHandler();

            return new HttpClient(httpMessageHandler);
        }
    }
}