using System;
using System.Threading;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public abstract class TestBase<T> : IDisposable, IClassFixture<T> where T : class, IAppHost
    {
        protected T WebFixture;

        protected TestBase(T webFixture, ITestOutputHelper output)
        {
            Output = output;
            WebFixture = webFixture;

            CancellationTokenSource = WebFixture?.App?.CancellationTokenSource;

            if (webFixture.Exception != null)
            {
                output.WriteLine(webFixture.Builder.ToString());
                output.WriteLine(webFixture.Exception.ToString());
            }
        }

        public ITestOutputHelper Output { get; }

        protected CancellationTokenSource CancellationTokenSource { get; }

        public virtual void Dispose()
        {
            if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel(false);
            }

            if (WebFixture != null)
            {
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