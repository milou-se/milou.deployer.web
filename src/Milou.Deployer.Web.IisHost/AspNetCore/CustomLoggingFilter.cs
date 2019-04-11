using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Http;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class CustomLoggingFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return builder => { };
        }
    }
}
