using System;

namespace Milou.Deployer.Web.Core
{
    public interface ITime
    {
        DateTimeOffset UtcNow();

        DateTime LocalNow();
    }
}
