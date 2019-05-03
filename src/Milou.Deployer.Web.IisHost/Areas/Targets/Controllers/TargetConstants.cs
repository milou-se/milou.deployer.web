using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.Controllers
{
    public static class TargetConstants
    {
        public const string RemoveTargetPostRoute = "/target/remove";

        public const string RemoveTargetPostRouteName = nameof(RemoveTargetPostRoute);

        public const string EditTargetPostRoute = "/target";

        public const string EditTargetPostRouteName = nameof(EditTargetPostRoute);

        public const string CreateTargetPostRoute = "/targets";

        public const string CreateTargetPostRouteName = nameof(CreateTargetPostRoute);

        public const string AreaName = "Targets";

        [PublicAPI]
        public const string TargetsBaseRoute = "/organizations/{organizationId}/{project}";

        [PublicAPI]
        public const string TargetsBaseRouteName = nameof(TargetsBaseRoute);

        public const string TargetsRoute = "/targets";

        public const string TargetsRouteName = nameof(TargetsRoute);

        public const string CreateTargetGetRoute = "/targets/create";

        public const string CreateTargetGetRouteName = nameof(CreateTargetGetRoute);

        public const string EditTargetRoute = "/target/{targetId}/edit";

        public const string EditTargetRouteName = nameof(EditTargetRoute);

        public const string InvalidateCacheRoute = "/invalidatecache";

        public const string InvalidateCacheRouteName = nameof(InvalidateCacheRoute);

        public const string EnableTargetPostRoute = "/target/enable";

        public const string EnableTargetPostRouteName = nameof(EnableTargetPostRoute);

        public const string DisabledTargetsRoute = "/targets/enabled";

        public const string DisabledTargetsRouteName = nameof(DisabledTargetsRoute);
    }
}
