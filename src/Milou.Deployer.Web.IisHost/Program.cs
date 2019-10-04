using System.Threading.Tasks;
using Arbor.Primitives;

namespace Milou.Deployer.Web.IisHost
{
    internal static class Program
    {
        public static Task<int> Main(string[] args) => AppStarter.StartAsync(args, EnvironmentVariables.GetEnvironmentVariables().Variables);
    }
}
