using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Milou.Deployer.Web.Core;
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

        [Fact(Skip = "Issues with postgresql permissions")]
        public async Task Then_It_Should_Return_Html_In_Response_Body()
        {
            var headers = string.Join(Environment.NewLine,
                WebFixture?.ResponseMessage?.Headers?.Select(pair => $"{pair.Key}:{string.Join(",", pair.Value)}") ??
                Array.Empty<string>());

            Output.WriteLine($"Response status: {WebFixture?.ResponseMessage?.StatusCode}");

            Output.WriteLine($"Response headers: {headers}");

            var body = WebFixture?.ResponseMessage?.Content != null
                ? await WebFixture.ResponseMessage.Content?.ReadAsStringAsync()
                : Constants.NotAvailable;
            Output.WriteLine($"Response body: {body}");

            Assert.Contains("<html", body, StringComparison.Ordinal);
        }


        [Fact(Skip = "Issues with postgresql permissions")]
        public void ThenItShouldReturnHttpStatusCodeOk200()
        {
            Output.WriteLine($"Response status code {WebFixture?.ResponseMessage?.StatusCode}");

            Assert.Equal(HttpStatusCode.OK, WebFixture?.ResponseMessage?.StatusCode);
        }
    }
}
