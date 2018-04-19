namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class WorkTaskStatus
    {
        private WorkTaskStatus()
        {

        }

        public static readonly WorkTaskStatus Started = new WorkTaskStatus();

        public static readonly WorkTaskStatus Enqueued = new WorkTaskStatus();

        public static readonly WorkTaskStatus Created = new WorkTaskStatus();

        public static readonly WorkTaskStatus Done = new WorkTaskStatus();

        public static readonly WorkTaskStatus Failed = new WorkTaskStatus();
    }
}