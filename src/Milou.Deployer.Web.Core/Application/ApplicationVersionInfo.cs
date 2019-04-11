using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Application
{
    public class ApplicationVersionInfo
    {
        public ApplicationVersionInfo(
            [NotNull] string assemblyVersion,
            [NotNull] string fileVersion,
            [NotNull] string informationalVersion,
            [NotNull] string assemblyFullName)
        {
            if (string.IsNullOrWhiteSpace(assemblyVersion))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(assemblyVersion));
            }

            if (string.IsNullOrWhiteSpace(fileVersion))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileVersion));
            }

            if (string.IsNullOrWhiteSpace(informationalVersion))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(informationalVersion));
            }

            if (string.IsNullOrWhiteSpace(assemblyFullName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(assemblyFullName));
            }

            AssemblyVersion = assemblyVersion;
            FileVersion = fileVersion;
            InformationalVersion = informationalVersion;
            AssemblyFullName = assemblyFullName;
        }

        public string AssemblyVersion { get; }

        public string FileVersion { get; }

        public string InformationalVersion { get; }

        public string AssemblyFullName { get; }
    }
}
