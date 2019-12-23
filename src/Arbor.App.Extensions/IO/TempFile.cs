using System;
using System.IO;

namespace Arbor.App.Extensions.IO
{
    public sealed class TempFile : IDisposable
    {
        private readonly DirectoryInfo _customTempDir;

        private TempFile(FileInfo file, DirectoryInfo customTempDir)
        {
            _customTempDir = customTempDir;
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        public FileInfo File { get; private set; }

        public static TempFile CreateTempFile(string name = null, string extension = null)
        {
            string defaultName = $"MDW-tmp-{DateTime.UtcNow.Ticks}";

            string fileName = $"{name.WithDefault(defaultName)}.{extension?.TrimStart('.').WithDefault("tmp")}";

            string tempDir = Path.GetTempPath();

            DirectoryInfo customTempDir = default;

            if (!string.IsNullOrWhiteSpace(name))
            {
                tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                customTempDir = new DirectoryInfo(tempDir);

                customTempDir.Create();
            }

            string fileFullPath = Path.Combine(tempDir, fileName);

            using (System.IO.File.Create(fileFullPath))
            {
            }

            var fileInfo = new FileInfo(fileFullPath);

            return new TempFile(fileInfo, customTempDir);
        }

        public void Dispose()
        {
            try
            {
                if (File != null)
                {
                    File.Refresh();

                    if (File.Exists)
                    {
                        File.Delete();
                    }
                }

                if (_customTempDir != null)
                {
                    _customTempDir.Refresh();
                    if (_customTempDir.Exists)
                    {
                        _customTempDir.Delete(true);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            File = null;
        }
    }
}