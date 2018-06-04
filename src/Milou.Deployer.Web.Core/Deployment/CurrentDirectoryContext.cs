using System;
using System.IO;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class CurrentDirectoryContext : IDisposable
    {
        private readonly DirectoryInfo _oldCurrentDirectory;

        private CurrentDirectoryContext([NotNull] DirectoryInfo currentDirectory)
        {
            if (currentDirectory == null)
            {
                throw new ArgumentNullException(nameof(currentDirectory));
            }

            _oldCurrentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            Directory.SetCurrentDirectory(currentDirectory.FullName);
        }

        public static CurrentDirectoryContext Create([NotNull] DirectoryInfo currentDirectory)
        {
            if (currentDirectory == null)
            {
                throw new ArgumentNullException(nameof(currentDirectory));
            }

            return new CurrentDirectoryContext(currentDirectory);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_oldCurrentDirectory.FullName);
        }
    }
}