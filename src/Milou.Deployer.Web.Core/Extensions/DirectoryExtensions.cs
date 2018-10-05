using System;
using System.IO;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static  class DirectoryExtensions
    {
        public static DirectoryInfo EnsureExists([NotNull] this DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }

            directoryInfo.Refresh();

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            directoryInfo.Refresh();

            return directoryInfo;
        }
    }
}