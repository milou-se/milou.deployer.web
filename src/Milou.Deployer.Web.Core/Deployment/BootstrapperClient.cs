using System;
using System.Net.Http;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class BootstrapperClient
    {
        public BootstrapperClient([NotNull] HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public HttpClient HttpClient { get; }
    }
}