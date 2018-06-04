using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IsFatal(this Exception ex)
        {
            if (ex == null)
            {
                return false;
            }

            return
                ex is OutOfMemoryException ||
                ex is AccessViolationException ||
                ex is AppDomainUnloadedException ||
                ex is StackOverflowException ||
                ex is ThreadAbortException ||
                ex is SEHException;
        }
    }
}