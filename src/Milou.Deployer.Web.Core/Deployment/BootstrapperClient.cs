using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class BootstrapperClient
    {
        [NotNull]
        public CustomHttpClientFactory HttpClientFactory { get; }

        public BootstrapperClient([NotNull] CustomHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }
    }
}