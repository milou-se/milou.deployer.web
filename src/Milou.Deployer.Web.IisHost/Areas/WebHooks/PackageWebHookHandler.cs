using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.ExtensionMethods;
using MediatR;

using Microsoft.AspNetCore.Http;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    public class PackageWebHookHandler {
        private readonly ILogger _logger;

        private readonly ImmutableArray<IPackageWebHook> _packageWebHooks;

        private readonly IMediator _mediator;

        public PackageWebHookHandler(
            IEnumerable<IPackageWebHook> packageWebHooks,
            ILogger logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            _packageWebHooks = packageWebHooks.SafeToImmutableArray();
        }

        public async Task<WebHookResult> HandleRequest(
            HttpRequest request,
            string content,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.Debug("Cannot process empty web hook request body");
                return new WebHookResult(false);
            }

            bool handled = false;

            foreach (var packageWebHook in _packageWebHooks)
            {
                CancellationTokenSource? cancellationTokenSource = default;

                if (cancellationToken == CancellationToken.None)
                {
                    cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    cancellationToken = cancellationTokenSource.Token;
                }

                try
                {
                    var webHookNotification =
                        await packageWebHook.TryGetWebHookNotification(request, content, cancellationToken);

                    if (webHookNotification is null)
                    {
                        continue;
                    }

                    handled = true;

                    _logger.Information("Web hook successfully handled by {Handler}", packageWebHook.GetType().FullName);

                    await Task.Run(() => _mediator.Publish(webHookNotification, cancellationToken), cancellationToken);

                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "Could not get web hook notification from hook {Hook}",
                        packageWebHook.GetType().FullName);
                    throw;
                }
                finally
                {
                    cancellationTokenSource?.Dispose();
                }
            }

            return new WebHookResult(handled);
        }
    }
}