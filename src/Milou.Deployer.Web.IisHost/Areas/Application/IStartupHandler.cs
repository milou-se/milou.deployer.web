using System.Threading.Tasks;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    internal interface IStartupHandler
    {
        Task HandleAsync();
    }
}