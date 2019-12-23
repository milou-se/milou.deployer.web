﻿using System;
using System.Threading;
using Arbor.App.Extensions.Configuration;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public abstract class TestBase<T> : IDisposable, IClassFixture<T> where T : class, IAppHost
    {
        protected T WebFixture { get; private set; }

        protected TestBase([NotNull] T webFixture, [NotNull] ITestOutputHelper output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            WebFixture = webFixture ?? throw new ArgumentNullException(nameof(webFixture));
            webFixture.App?.ConfigurationInstanceHolder?.AddInstance(output);

            CancellationTokenSource = WebFixture?.App?.CancellationTokenSource;

            if (webFixture.Exception != null)
            {
                output.WriteLine(webFixture.Exception.ToString());
            }
        }

        [PublicAPI]
        public ITestOutputHelper Output { get; }

        [PublicAPI]
        protected CancellationTokenSource CancellationTokenSource { get; }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            Output?.WriteLine($"Disposing {nameof(TestBase<T>)}");

            if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    CancellationTokenSource.Cancel(false);
                }
                catch (ObjectDisposedException)
                {
                }
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
