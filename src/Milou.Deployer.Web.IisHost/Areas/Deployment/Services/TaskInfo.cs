using System;

using NuGet.Versioning;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class TaskInfo
    {
        public SemanticVersion SemanticVersion { get; }

        public DateTime EnqueuedAtUtc { get; }

        public TaskInfo(SemanticVersion semanticVersion, DateTime enqueuedAtUtc)
        {
            SemanticVersion = semanticVersion;
            EnqueuedAtUtc = enqueuedAtUtc;
        }
    }
}