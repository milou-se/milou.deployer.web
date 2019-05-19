﻿using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewInputModels
{
    [PublicAPI]
    public class DeploymentTaskInput
    {
        public string TargetId { get; set; }

        public string PackageId { get; set; }

        public string Version { get; set; }
    }
}
