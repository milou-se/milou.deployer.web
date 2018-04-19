using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Application
{
    public class ApplicationEnvironment
    {
        public string BasePath { get; }

        public ApplicationEnvironment([NotNull] string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(basePath));
            }

            BasePath = basePath;
        }
    }
}