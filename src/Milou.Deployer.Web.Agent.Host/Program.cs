using System.Threading.Tasks;
using Arbor.AspNetCore.Host;
using Arbor.Primitives;

namespace Milou.Deployer.Web.Agent.Host
{
    internal static class Program
    {
        public static Task<int> Main(string[] args) =>
            AppStarter<AgentStartup>.StartAsync(args, EnvironmentVariables.GetEnvironmentVariables().Variables);
    }
}