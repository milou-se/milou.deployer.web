using MediatR;

namespace Milou.Deployer.Web.Core.Logging
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
