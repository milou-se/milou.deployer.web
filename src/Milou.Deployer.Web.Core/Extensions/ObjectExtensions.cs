using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Extensions
{
    [PublicAPI]
    public static class ObjectExtensions
    {
        public static void DisposeIfPossible(this object item)
        {
            if (item is null)
            {
                return;
            }

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}