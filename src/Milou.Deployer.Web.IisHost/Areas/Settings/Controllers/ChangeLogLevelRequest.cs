using MediatR;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ChangeLogLevelRequest : IRequest
    {
        public ChangeLogLevelRequest(ChangeLogLevel changeLogLevel)
        {
            ChangeLogLevel = changeLogLevel;
        }

        public ChangeLogLevel ChangeLogLevel { get; }
    }
}
