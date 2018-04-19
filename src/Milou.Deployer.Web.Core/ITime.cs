using System;

namespace Milou.Deployer.Web.Core
{
    public interface ITime
    {
        DateTime UtcNow();

        DateTime LocalNow();
    }
}
