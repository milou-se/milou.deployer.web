using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Http;

using Milou.Deployer.Web.Core.Extensions;

using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    public class PackageWebHookHandler {
        private readonly ILogger _logger;

        private readonly ImmutableArray<IPackageWebHook> _packageWebHooks;

        private readonly IMediator _mediator;

        public PackageWebHookHandler(IEnumerable<IPackageWebHook> packageWebHooks, ILogger logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            _packageWebHooks = packageWebHooks.SafeToImmutableArray();
        }

        public async Task HandleRequest(HttpRequest request)
        {
            request.EnableBuffering();

            foreach (var packageWebHook in _packageWebHooks)
            {
                request.Body.Position = 0;

                try
                {
                    var webHookNotification = await packageWebHook.TryGetWebHookNotification(request);

                    if (webHookNotification is null)
                    {
                        continue;
                    }

                    await Task.Run(() => _mediator.Publish(webHookNotification));

                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not get web hook notification from hook {Hook}", packageWebHook.GetType().FullName);
                }
            }
        }
    }
}