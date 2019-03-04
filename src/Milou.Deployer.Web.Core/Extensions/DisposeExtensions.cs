using System;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class DisposeExtensions
    {
        public static void SafeDispose(this object disposable)
        {
            if (disposable is null)
            {
                return;
            }

            if (!(disposable is IDisposable disposableItem))
            {
                return;
            }

            try
            {
                disposableItem.Dispose();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
