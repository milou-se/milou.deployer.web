﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class WhenMakingHttpGetRequestToRoot : TestBase<HttpGetRequestToRoot>
    {
        public WhenMakingHttpGetRequestToRoot(
            HttpGetRequestToRoot webFixture,
            ITestOutputHelper output) : base(webFixture, output)
        {
        }

        [Fact]
        public async Task Then_It_Should_Return_Html_In_Response_Body()
        {
            string headers = string.Join(Environment.NewLine,
                WebFixture.ResponseMessage.Headers.Select(pair => $"{pair.Key}:{string.Join(",", pair.Value)}"));
            Output.WriteLine($"Response headers: {headers}");

            string body = await WebFixture.ResponseMessage.Content.ReadAsStringAsync();
            Output.WriteLine($"Response body: {body}");

            Assert.Contains("<html", body);
        }

        [Fact]
        public void ThenItShouldReturnHttpStatusCodeOk200()
        {
            Output.WriteLine($"Response status code {WebFixture?.ResponseMessage?.StatusCode}");

            Assert.Equal(HttpStatusCode.OK, WebFixture?.ResponseMessage?.StatusCode);
        }
    }
}
