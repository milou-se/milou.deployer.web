using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.App.Extensions;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Startup
{
    public class StartupTaskContext
    {
        private readonly ImmutableArray<IStartupTask> _startupTasks;

        private bool _isCompleted;

        public StartupTaskContext(IEnumerable<IStartupTask> startupTasks)
        {
            _startupTasks = startupTasks.SafeToImmutableArray();
        }

        public bool IsCompleted
        {
            get
            {
                if (_isCompleted)
                {
                    return true;
                }

                var isCompleted = _startupTasks.All(task => task.IsCompleted);

                _isCompleted = isCompleted;

                return isCompleted;
            }
        }
    }
}
