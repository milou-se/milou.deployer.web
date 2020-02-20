using System;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Agent.Host
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var exitCode = await AgentApp.CreateAndRunAsync(args);

            return exitCode;
        }


    }
}
