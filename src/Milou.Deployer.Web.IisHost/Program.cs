using System.Threading.Tasks;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost
{
    internal static class Program
    {
        public static Task<int> Main(string[] args)
        {
            return AppStarter.StartAsync(args, EnvironmentVariables.Get());
        }
    }
}
