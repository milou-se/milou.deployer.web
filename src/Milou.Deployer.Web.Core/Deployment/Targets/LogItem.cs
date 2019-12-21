using System;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class LogItem
    {
        public string TaskLogId { get; set; }

        public string Message { get; set; }

        public int Level { get; set; }

        public string Id { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}