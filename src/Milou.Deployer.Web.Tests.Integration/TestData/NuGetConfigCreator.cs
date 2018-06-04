using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Milou.Deployer.Web.Tests.Integration.TestData
{
    public class NuGetConfigCreator
    {
        public static async Task CreateNuGetConfig(
            FileInfo filePath,
            DirectoryInfo packageDirectory,
            CancellationToken cancellationToken = default)
        {
            var doc = new XDocument();

            var packageSourceElement = new XElement("add");
            packageSourceElement.Add(new XAttribute("key", "Milou.Deployer.Web.Tests.Integration"));
            packageSourceElement.Add(new XAttribute("value", packageDirectory.FullName));

            var packageSourcesElement = new XElement("packageSources");
            packageSourcesElement.Add(new XElement("clear"));
            packageSourcesElement.Add(packageSourceElement);

            var configurationElement = new XElement("configuration");

            configurationElement.Add(packageSourcesElement);

            doc.Add(configurationElement);

            using (var fileStream = new FileStream(filePath.FullName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await doc.SaveAsync(fileStream, SaveOptions.None, cancellationToken);
            }
        }
    }
}