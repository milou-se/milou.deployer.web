using System.IO;
using Arbor.Aesculus.Core;

namespace Milou.Deployer.Web.Tests.Integration
{
    public static class VcsTestPathHelper
    {
        public static string GetRootDirectory()
        {
            string originalSolutionPath = NCrunch.Framework.NCrunchEnvironment.GetOriginalSolutionPath();

            if (!string.IsNullOrWhiteSpace(originalSolutionPath))
            {
                var fileInfo = new FileInfo(originalSolutionPath);
                return VcsPathHelper.FindVcsRootPath(fileInfo.Directory?.FullName ?? Directory.GetCurrentDirectory());
            }

            return VcsPathHelper.FindVcsRootPath(Directory.GetCurrentDirectory());
        }
    }
}