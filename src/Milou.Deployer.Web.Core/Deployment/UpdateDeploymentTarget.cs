using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.Extensions.Primitives;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class UpdateDeploymentTarget : IRequest<UpdateDeploymentTargetResult>
    {
        public UpdateDeploymentTarget(
            string id,
            StringValues allowedPackageNames,
            bool allowExplicitPreRelease,
            Uri url,
            string iisSiteName = null,
            string nugetPackageSource = null,
            string nugetConfigFile = null)
        {
            Id = id;
            AllowedPackageNames = allowedPackageNames;
            AllowExplicitPreRelease = allowExplicitPreRelease;
            Url = url;
            IisSiteName = iisSiteName;
            NugetPackageSource = nugetPackageSource;
            NugetConfigFile = nugetConfigFile;
        }

        public string Id { get; }

        public ICollection<string> AllowedPackageNames { get; }

        public Uri Url { get; }

        public bool AllowExplicitPreRelease { get; }

        public string IisSiteName { get; }
        public string NugetPackageSource { get; }
        public string NugetConfigFile { get; }
    }
}