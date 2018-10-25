﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class HttpGetRequestToRoot : WebFixtureBase, IAppHost
    {
        public HttpGetRequestToRoot(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        {
        }

        public HttpResponseMessage ResponseMessage { get; private set; }

        protected override async Task RunAsync()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    ResponseMessage = await httpClient.GetAsync($"http://localhost:{HttpPort}", CancellationToken);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                   App?.Logger?.Error(ex, "Error in test");
                    Assert.NotNull(ex);
                }
            }
        }
    }
}
