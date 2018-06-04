namespace Milou.Deployer.Web.IisHost.Areas.Projects
{
    public static class ProjectConstants
    {
        public const string CreateProjectPostRouteName = nameof(CreateProjectPostRoute);
        public const string CreateProjectPostRoute = "/projects";
        public const string AreaName = "Projects";
        public const string ProjectsBaseRoute = "/organizations/{organizationId}/projects";
        public const string ProjectsBaseRouteName = nameof(ProjectsBaseRoute);
    }
}