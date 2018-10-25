using System;
using System.IO;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class DirectoryExtensions
    {
        public static bool TryEnsureDirectoryExists([NotNull] this string directory, out DirectoryInfo directoryInfo)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));
            }

            try
            {
                directoryInfo = new DirectoryInfo(directory).EnsureExists();

                return true;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                directoryInfo = null;
                return false;
            }
        }

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