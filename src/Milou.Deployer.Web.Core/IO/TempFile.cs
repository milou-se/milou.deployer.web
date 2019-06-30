using System;
using System.IO;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.IO
{
    public sealed class TempFile : IDisposable
    {
        private TempFile(FileInfo file) => File = file ?? throw new ArgumentNullException(nameof(file));

        public FileInfo File { get; private set; }

        public static TempFile CreateTempFile(string name = null, string extension = null)
        {
            string fileName = $"{name.WithDefault("MDW-tmp")}-{DateTime.UtcNow.Ticks}.{extension.WithDefault(".tmp")}";

            string fileFullPath = Path.Combine(Path.GetTempPath(), fileName);

            return new TempFile(new FileInfo(fileFullPath));
        }

        public void Dispose()
        {
            if (File == null)
            {
                return;
            }

            File.Refresh();

            if (File.Exists)
            {
                File.Delete();
            }

            File = null;
        }
    }
}
