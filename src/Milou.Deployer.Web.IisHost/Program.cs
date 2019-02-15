using System.Threading.Tasks;

namespace Milou.Deployer.Web.IisHost
{
    internal static class Program
    {
        public static Task<int> Main(string[] args)
        {
            return AppStarter.StartAsync(args);
        }
    }
}
