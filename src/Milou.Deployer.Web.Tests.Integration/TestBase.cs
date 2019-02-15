using System;
using System.Threading;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public abstract class TestBase<T> : IDisposable, IClassFixture<T> where T : class, IAppHost
    {
        protected T WebFixture;

        protected TestBase([NotNull] T webFixture, [NotNull] ITestOutputHelper output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            WebFixture = webFixture ?? throw new ArgumentNullException(nameof(webFixture));

            CancellationTokenSource = WebFixture?.App?.CancellationTokenSource;

            if (webFixture.Exception != null)
            {
                if (webFixture.Builder != null)
                {
                    output.WriteLine(webFixture.Builder?.ToString() ?? "");
                }

                output.WriteLine(webFixture.Exception.ToString());
            }
        }

        [PublicAPI]
        public ITestOutputHelper Output { get; }

        [PublicAPI]
        protected CancellationTokenSource CancellationTokenSource { get; }

        public virtual void Dispose()
        {
            Output?.WriteLine($"Disposing {nameof(TestBase<T>)}");

            if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel(false);
            }

            if (WebFixture != null)
            {
                Output?.WriteLine($"Disposing {WebFixture}");

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