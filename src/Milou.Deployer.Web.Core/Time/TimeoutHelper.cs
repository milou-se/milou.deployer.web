using System;
using System.Threading;

namespace Milou.Deployer.Web.Core.Time
{
    public class TimeoutHelper
    {
        private readonly TimeoutConfiguration _timeoutConfiguration;

        public TimeoutHelper(TimeoutConfiguration timeoutConfiguration = null)
        {
            _timeoutConfiguration = timeoutConfiguration;
        }

        public CancellationTokenSource CreateCancellationTokenSource(TimeSpan timeSpan)
        {
            if (_timeoutConfiguration?.CancellationEnabled == false)
            {
                return new CancellationTokenSource();
            }

            return new CancellationTokenSource(timeSpan);
        }
    }
}
