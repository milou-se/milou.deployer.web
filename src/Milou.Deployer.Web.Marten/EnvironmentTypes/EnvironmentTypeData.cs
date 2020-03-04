using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten.EnvironmentTypes
{
    [MartenData]
    public class EnvironmentTypeData
    {
        public string PreReleaseBehavior { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public static readonly EnvironmentTypeData Empty = new EnvironmentTypeData {Id = ""};

        public static EnvironmentTypeData MapToData(EnvironmentType environmentType) =>
            new EnvironmentTypeData
            {
                Id = environmentType.Id.Trim(),
                PreReleaseBehavior = environmentType.PreReleaseBehavior.Name.Trim(),
                Name = environmentType.Name
            };

        public static EnvironmentType MapFromData(EnvironmentTypeData data) => new EnvironmentType(data.Id, data.Name,
            Core.Deployment.PreReleaseBehavior.Parse(data.PreReleaseBehavior));
    }
}