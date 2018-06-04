using System;
using System.Threading;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public abstract class TestBase<T> : IDisposable, IClassFixture<T> where T : class, IAppHost
    {
        protected ILogger Logger;
        protected T WebFixture;

        protected TestBase(T webFixture, ITestOutputHelper output)
        {
            WebFixture = webFixture;

            Logger = WebFixture.App.Logger;
            CancellationTokenSource = WebFixture.App.CancellationTokenSource;

            if (webFixture.Exception != null)
            {
                output.WriteLine(webFixture.Builder.ToString());
                output.WriteLine(webFixture.Exception.ToString());
            }
        }

        protected CancellationTokenSource CancellationTokenSource { get; }

        public virtual void Dispose()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel(throwOnFirstException: false);
            }

            if (WebFixture != null)
            {
                Logger = null;
                WebFixture.App?.Dispose();

                if (WebFixture is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                WebFixture = null;
            }


        }
    }
}