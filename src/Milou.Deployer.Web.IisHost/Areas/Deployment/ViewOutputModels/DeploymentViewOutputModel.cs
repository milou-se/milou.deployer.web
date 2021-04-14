using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.ExtensionMethods;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Packages;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class DeploymentViewOutputModel
    {
        public DeploymentViewOutputModel(
            [NotNull] IReadOnlyCollection<PackageVersion> packageVersions,
            [NotNull] IReadOnlyCollection<DeploymentTarget> targets)
        {
            if (packageVersions == null)
            {
                throw new ArgumentNullException(nameof(packageVersions));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            PackageVersions =
                packageVersions.OrderBy(packageVersion => packageVersion.PackageId).SafeToReadOnlyCollection();
            Targets = targets.OrderBy(target => target.Name).SafeToReadOnlyCollection();
        }

        public IReadOnlyCollection<PackageVersion> PackageVersions { get; }

        public IReadOnlyCollection<DeploymentTarget> Targets { get; }
    }
}
