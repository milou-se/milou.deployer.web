namespace Milou.Deployer.Web.Core.Deployment.WorkTasks
{
    public sealed class WorkTaskStatus
    {
        public static readonly WorkTaskStatus Started = new WorkTaskStatus(nameof(Started));

        public static readonly WorkTaskStatus Enqueued = new WorkTaskStatus(nameof(Enqueued));

        public static readonly WorkTaskStatus Created = new WorkTaskStatus(nameof(Created));

        public static readonly WorkTaskStatus Done = new WorkTaskStatus(nameof(Done));

        public static readonly WorkTaskStatus Failed = new WorkTaskStatus(nameof(Failed));

        private WorkTaskStatus(string status) => Status = status;

        public string Status { get; }

        public override string ToString() => Status;
    }
}