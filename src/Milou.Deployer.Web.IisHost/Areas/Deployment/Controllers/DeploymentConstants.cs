namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    public static class DeploymentConstants
    {
        public const string AreaName = "Deployment";

        public const string HistoryRoute = "/deployment/{deploymentTargetId}/history";

        public const string HistoryRouteName = nameof(HistoryRoute);

        public const string HistoryLogRoute = "/log/{deploymentTaskId}";

        public const string HistoryLogRouteName = nameof(HistoryLogRoute);

        public const string DeployRoute = "/deploy";

        public const string DeployRouteName = nameof(DeployRoute);

        public const string DeploymentStatusRoute = "/deployment/status/{deploymentTargetId}";

        public const string DeploymentStatusRouteName = nameof(DeploymentStatusRoute);
    }
}
