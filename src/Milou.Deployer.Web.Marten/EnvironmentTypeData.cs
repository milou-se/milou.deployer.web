using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten
{
    public class EnvironmentTypeData
    {
        public string PreReleaseBehavior { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public static EnvironmentTypeData MapToData(EnvironmentType environmentType) =>
            new EnvironmentTypeData
            {
                Id = environmentType.Id,
                PreReleaseBehavior = environmentType.PreReleaseBehavior.Name,
                Name = environmentType.Name
            };

        public static EnvironmentType MapFromData(EnvironmentTypeData data) => new EnvironmentType(data.Id, data.Name,
            Core.Deployment.PreReleaseBehavior.Parse(data.PreReleaseBehavior));
    }
}