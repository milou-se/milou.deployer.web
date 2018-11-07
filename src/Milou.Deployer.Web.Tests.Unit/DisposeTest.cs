using System;
using Milou.Deployer.Web.Core.Extensions;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class DisposeTest
    {
        [Fact]
        public void DisposeNonDisposable()
        {
            var o = new object();
            o.SafeDispose();
        }

        [Fact]
        public void DisposeDisposable()
        {
            var o = new TestDisposable();
            o.SafeDispose();
        }

        [Fact]
        public void DisposeNull()
        {
            object o = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            o.SafeDispose();
        }

        class TestDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
        class ThrowingTestDisposable : IDisposable
        {
            public void Dispose()
            {
                throw new InvalidOperationException("test exception");
            }
        }
    }
}